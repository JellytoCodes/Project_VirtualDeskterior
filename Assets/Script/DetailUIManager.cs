using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DetailUIManager : MonoBehaviour
{
    private static Sprite runtimeSolidButtonSprite;

    [Header("System Managers")]
    public JelliMetaUIManager mainUIManager;

    [Header("Detail UI Elements")]
    public Image detailProductImage;
    public TextMeshProUGUI detailProductNameText;
    public TextMeshProUGUI detailPriceText;
    public TextMeshProUGUI detailDescriptionText;

    [Header("Detail Buttons")]
    public Button backButton;
    public Button openARButton;

    private TextMeshProUGUI dimensionText;
    private TextMeshProUGUI materialText;

    private void OnEnable()
    {
        if (mainUIManager == null)
            mainUIManager = FindAnyObjectByType<JelliMetaUIManager>();

        NormalizeDetailLayout();

        if (mainUIManager != null)
            SetupDetailPanel(mainUIManager.currentSelectedProduct);
        else
            Debug.LogError("[DetailUIManager] JelliMetaUIManager가 연결되지 않았습니다.");

        ForceActionButtonLabelColors();
    }

    private void LateUpdate()
    {
        // RuntimeButtonLabel 색상이 다른 런타임 보정에 의해 다시 흰색으로 돌아가는 것을 방지한다.
        ForceActionButtonLabelColors();
    }

    private void SetupDetailPanel(ProductData data)
    {
        if (detailProductImage != null)
        {
            detailProductImage.sprite = data.productImage;
            detailProductImage.preserveAspect = true;
        }

        if (detailProductNameText != null)
            JelliMetaUIRuntimeBuilder.ConfigureText(detailProductNameText, string.IsNullOrEmpty(data.productName) ? "상품명 없음" : data.productName, 56f, FontStyles.Bold, Color.black, TextAlignmentOptions.Left);

        if (detailPriceText != null)
            JelliMetaUIRuntimeBuilder.ConfigureText(detailPriceText, string.IsNullOrEmpty(data.price) ? "가격 정보 없음" : data.price, 44f, FontStyles.Bold, Color.black, TextAlignmentOptions.Left);

        if (detailDescriptionText != null)
            JelliMetaUIRuntimeBuilder.ConfigureText(detailDescriptionText, string.IsNullOrEmpty(data.description) ? "상품 설명이 아직 등록되지 않았습니다." : data.description, 31f, FontStyles.Normal, JelliMetaUIRuntimeBuilder.Color32(75, 85, 99), TextAlignmentOptions.Left);

        EnsureMetaTextObjects();

        if (dimensionText != null)
            JelliMetaUIRuntimeBuilder.ConfigureText(dimensionText, $"크기\n{SafeText(data.dimensions, "정보 없음")}", 30f, FontStyles.Normal, JelliMetaUIRuntimeBuilder.Color32(75, 85, 99), TextAlignmentOptions.Left);

        if (materialText != null)
            JelliMetaUIRuntimeBuilder.ConfigureText(materialText, $"소재\n{SafeText(data.material, "정보 없음")}", 30f, FontStyles.Normal, JelliMetaUIRuntimeBuilder.Color32(75, 85, 99), TextAlignmentOptions.Left);

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => mainUIManager.ShowHomePanel());
        }

        if (openARButton != null)
        {
            openARButton.onClick.RemoveAllListeners();
            openARButton.onClick.AddListener(() =>
            {
                Debug.Log($"[DetailUIManager] AR 버튼 클릭: {data.productName}");
                mainUIManager.ShowARPlacementScreen(data);
            });
        }
        else
        {
            Debug.LogError("[DetailUIManager] openARButton이 연결되지 않았습니다.");
        }

        JelliMetaUIRuntimeBuilder.ApplyProjectFonts(gameObject);
        ApplyActionButtonVisuals();
    }

    private void NormalizeDetailLayout()
    {
        JelliMetaUIRuntimeBuilder.ConfigureCanvasForMobile(gameObject);

        RectTransform detailPanelRect = transform as RectTransform;
        JelliMetaUIRuntimeBuilder.SetStretch(detailPanelRect, 0f, 0f, 0f, 0f);

        ForceWhiteBackground(gameObject);

        Transform header = transform.Find("HeaderArea");
        if (header != null)
        {
            JelliMetaUIRuntimeBuilder.SetTop(header as RectTransform, 0f, 0f, 0f, 108f);
            ForceWhiteBackground(header.gameObject);
        }

        if (backButton != null)
        {
            JelliMetaUIRuntimeBuilder.SetFixed(backButton.transform as RectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -22f), new Vector2(210f, 64f));
            EnsureButtonVisual(backButton, "뒤로", Color.clear, Color.black, 30f, TextAlignmentOptions.Left, 0f, 0f, 0f, 0f);
        }

        Transform bottom = transform.Find("BottomActionArea");
        if (bottom != null)
        {
            JelliMetaUIRuntimeBuilder.SetBottom(bottom as RectTransform, 0f, 0f, 0f, 270f);
            ForceWhiteBackground(bottom.gameObject);
        }

        ApplyActionButtonVisuals();

        Button cartButton = transform.Find("BottomActionArea/CartButton")?.GetComponent<Button>();
        if (cartButton != null)
        {
            cartButton.onClick.RemoveAllListeners();
            cartButton.onClick.AddListener(() => Debug.Log("[DetailUIManager] 장바구니 담기 클릭"));
        }

        Transform middle = transform.Find("MiddleScrollArea");
        if (middle != null)
        {
            JelliMetaUIRuntimeBuilder.SetStretch(middle as RectTransform, 0f, 0f, 108f, 270f);
            ForceWhiteBackground(middle.gameObject);
        }

        Transform scrollView = transform.Find("MiddleScrollArea/DetailScrollView");
        if (scrollView != null)
        {
            JelliMetaUIRuntimeBuilder.SetStretch(scrollView as RectTransform, 0f, 0f, 0f, 0f);
            ForceWhiteBackground(scrollView.gameObject);
        }

        Transform viewport = transform.Find("MiddleScrollArea/DetailScrollView/Viewport");
        if (viewport != null)
            ForceWhiteBackground(viewport.gameObject);

        Transform content = transform.Find("MiddleScrollArea/DetailScrollView/Viewport/Content");
        if (content != null)
        {
            ForceWhiteBackground(content.gameObject);

            VerticalLayoutGroup contentLayout = JelliMetaUIRuntimeBuilder.EnsureComponent<VerticalLayoutGroup>(content.gameObject);
            contentLayout.padding = new RectOffset(0, 0, 0, 32);
            contentLayout.spacing = 36f;
            contentLayout.childAlignment = TextAnchor.UpperLeft;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;

            ContentSizeFitter fitter = JelliMetaUIRuntimeBuilder.EnsureComponent<ContentSizeFitter>(content.gameObject);
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        if (detailProductImage != null)
        {
            detailProductImage.color = Color.white;
            detailProductImage.preserveAspect = true;

            LayoutElement imageLayout = JelliMetaUIRuntimeBuilder.EnsureComponent<LayoutElement>(detailProductImage.gameObject);
            imageLayout.minHeight = 700f;
            imageLayout.preferredHeight = 760f;
            imageLayout.flexibleHeight = 0f;
        }

        RectTransform infoContainer = detailProductNameText != null ? detailProductNameText.transform.parent as RectTransform : null;
        if (infoContainer != null)
        {
            LayoutElement infoLayout = JelliMetaUIRuntimeBuilder.EnsureComponent<LayoutElement>(infoContainer.gameObject);
            infoLayout.minHeight = 520f;
            infoLayout.preferredHeight = 660f;
            infoLayout.flexibleHeight = 0f;

            VerticalLayoutGroup infoLayoutGroup = JelliMetaUIRuntimeBuilder.EnsureComponent<VerticalLayoutGroup>(infoContainer.gameObject);
            infoLayoutGroup.padding = new RectOffset(32, 32, 0, 0);
            infoLayoutGroup.spacing = 20f;
            infoLayoutGroup.childAlignment = TextAnchor.UpperLeft;
            infoLayoutGroup.childControlWidth = true;
            infoLayoutGroup.childControlHeight = true;
            infoLayoutGroup.childForceExpandWidth = true;
            infoLayoutGroup.childForceExpandHeight = false;
        }

        ConfigureLayoutElement(detailProductNameText, 80f);
        ConfigureLayoutElement(detailPriceText, 66f);
        ConfigureLayoutElement(detailDescriptionText, 150f);
        EnsureMetaTextObjects();
        ConfigureLayoutElement(dimensionText, 96f);
        ConfigureLayoutElement(materialText, 96f);

        JelliMetaUIRuntimeBuilder.ApplyProjectFonts(gameObject);

        if (detailPanelRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(detailPanelRect);
    }

    private void ApplyActionButtonVisuals()
    {
        if (openARButton != null)
        {
            JelliMetaUIRuntimeBuilder.SetTop(openARButton.transform as RectTransform, 32f, 32f, 32f, 86f);
            EnsureButtonVisual(
                openARButton,
                "내 공간에서 보기 (AR)",
                JelliMetaUIRuntimeBuilder.Color32(243, 244, 246),
                JelliMetaUIRuntimeBuilder.Color32(17, 24, 39),
                32f,
                TextAlignmentOptions.Center,
                24f, 24f, 0f, 0f
            );
        }

        Button cartButton = transform.Find("BottomActionArea/CartButton")?.GetComponent<Button>();
        if (cartButton != null)
        {
            JelliMetaUIRuntimeBuilder.SetTop(cartButton.transform as RectTransform, 32f, 32f, 136f, 86f);
            EnsureButtonVisual(
                cartButton,
                "장바구니 담기",
                JelliMetaUIRuntimeBuilder.Color32(243, 244, 246),
                JelliMetaUIRuntimeBuilder.Color32(17, 24, 39),
                32f,
                TextAlignmentOptions.Center,
                24f, 24f, 0f, 0f
            );
        }
    }

    private void ForceActionButtonLabelColors()
    {
        Color labelColor = JelliMetaUIRuntimeBuilder.Color32(17, 24, 39);

        if (openARButton != null)
            ForceRuntimeButtonLabelColor(openARButton, labelColor);

        Button cartButton = transform.Find("BottomActionArea/CartButton")?.GetComponent<Button>();
        if (cartButton != null)
            ForceRuntimeButtonLabelColor(cartButton, labelColor);
    }

    private void ForceRuntimeButtonLabelColor(Button button, Color color)
    {
        if (button == null)
            return;

        Transform labelTransform = button.transform.Find("RuntimeButtonLabel");
        if (labelTransform == null)
            return;

        TextMeshProUGUI label = labelTransform.GetComponent<TextMeshProUGUI>();
        if (label == null)
            return;

        label.color = color;
        label.alpha = 1f;
        label.canvasRenderer.SetAlpha(1f);
        label.SetAllDirty();
    }

    private void ForceWhiteBackground(GameObject target)
    {
        if (target == null)
            return;

        Image image = JelliMetaUIRuntimeBuilder.EnsureComponent<Image>(target);
        if (image == null)
            return;

        image.color = Color.white;
        image.raycastTarget = false;
    }

    private void EnsureButtonVisual(Button button, string labelText, Color backgroundColor, Color textColor, float fontSize, TextAlignmentOptions alignment, float leftPadding, float rightPadding, float topPadding, float bottomPadding)
    {
        if (button == null)
            return;

        ConfigureButtonBackground(button, backgroundColor);
        EnsureRuntimeButtonLabel(button, labelText, textColor, fontSize, alignment, leftPadding, rightPadding, topPadding, bottomPadding);
    }

    private void ConfigureButtonBackground(Button button, Color backgroundColor)
    {
        if (button == null)
            return;

        Image image = button.GetComponent<Image>();
        if (image == null)
            image = button.gameObject.AddComponent<Image>();

        // 클릭 판정은 Image의 raycastTarget에 맡기고, Button ColorTint가 색을 다시 덮어쓰지 않도록 Transition을 끈다.
        // Scene View의 빨간 X는 Gizmo 표시였으므로 여기서 Graphic 교체/커스텀 Graphic 추가는 하지 않는다.
        if (image.sprite == null)
        {
            image.sprite = JelliMetaUIRuntimeBuilder.SolidSprite;
            image.type = Image.Type.Simple;
        }

        image.enabled = true;
        image.color = backgroundColor;
        image.raycastTarget = true;

        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;
        button.interactable = true;
    }

    private void EnsureRuntimeButtonLabel(Button button, string labelText, Color textColor, float fontSize, TextAlignmentOptions alignment, float leftPadding, float rightPadding, float topPadding, float bottomPadding)
    {
        if (button == null)
            return;

        TextMeshProUGUI runtimeLabel = null;
        Transform runtimeLabelTransform = button.transform.Find("RuntimeButtonLabel");
        if (runtimeLabelTransform != null)
            runtimeLabel = runtimeLabelTransform.GetComponent<TextMeshProUGUI>();

        if (runtimeLabel == null)
        {
            GameObject labelObject = new GameObject("RuntimeButtonLabel", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            labelObject.transform.SetParent(button.transform, false);
            runtimeLabel = labelObject.GetComponent<TextMeshProUGUI>();
        }

        // 기존 버튼 안에 있던 Text/TMP가 흰색/잘못된 RectTransform으로 남아 있으면 새 라벨과 충돌한다.
        foreach (TextMeshProUGUI label in button.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (label != runtimeLabel)
                label.gameObject.SetActive(false);
        }

        foreach (UnityEngine.UI.Text legacyText in button.GetComponentsInChildren<UnityEngine.UI.Text>(true))
            legacyText.gameObject.SetActive(false);

        runtimeLabel.gameObject.SetActive(true);
        runtimeLabel.enabled = true;
        runtimeLabel.transform.SetAsLastSibling();

        RectTransform labelRect = runtimeLabel.rectTransform;
        JelliMetaUIRuntimeBuilder.SetStretch(labelRect, leftPadding, rightPadding, topPadding, bottomPadding);
        labelRect.localScale = Vector3.one;

        LayoutElement layoutElement = runtimeLabel.GetComponent<LayoutElement>();
        if (layoutElement != null)
            layoutElement.ignoreLayout = true;

        JelliMetaUIRuntimeBuilder.ConfigureText(runtimeLabel, labelText, fontSize, FontStyles.Bold, textColor, alignment);
        runtimeLabel.text = labelText;
        runtimeLabel.color = textColor;
        runtimeLabel.alpha = 1f;
        runtimeLabel.fontSize = fontSize;
        runtimeLabel.fontStyle = FontStyles.Bold;
        runtimeLabel.alignment = alignment;
        runtimeLabel.textWrappingMode = TextWrappingModes.NoWrap;
        runtimeLabel.overflowMode = TextOverflowModes.Overflow;
        runtimeLabel.raycastTarget = false;
        runtimeLabel.enableAutoSizing = false;
        runtimeLabel.canvasRenderer.SetAlpha(1f);
        runtimeLabel.SetAllDirty();

        RectTransform buttonRect = button.transform as RectTransform;
        if (buttonRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(buttonRect);
    }

    private void EnsureMetaTextObjects()
    {
        RectTransform infoContainer = detailProductNameText != null ? detailProductNameText.transform.parent as RectTransform : null;
        if (infoContainer == null)
            return;

        if (dimensionText == null)
        {
            Transform existing = infoContainer.Find("DimensionText");
            dimensionText = existing != null ? existing.GetComponent<TextMeshProUGUI>() : JelliMetaUIRuntimeBuilder.CreateText(infoContainer, "DimensionText", "", 30f, FontStyles.Normal, Color.black, TextAlignmentOptions.Left);
        }

        if (materialText == null)
        {
            Transform existing = infoContainer.Find("MaterialText");
            materialText = existing != null ? existing.GetComponent<TextMeshProUGUI>() : JelliMetaUIRuntimeBuilder.CreateText(infoContainer, "MaterialText", "", 30f, FontStyles.Normal, Color.black, TextAlignmentOptions.Left);
        }
    }

    private void ConfigureLayoutElement(TextMeshProUGUI text, float preferredHeight)
    {
        if (text == null)
            return;

        text.textWrappingMode = TextWrappingModes.Normal;
        LayoutElement layout = JelliMetaUIRuntimeBuilder.EnsureComponent<LayoutElement>(text.gameObject);
        layout.minHeight = preferredHeight;
        layout.preferredHeight = preferredHeight;
        layout.flexibleHeight = 0f;
    }

    private string SafeText(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }
}
