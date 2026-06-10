using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DetailUIManager : MonoBehaviour
{
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

        Image panelBg = GetComponent<Image>();
        if (panelBg != null)
            panelBg.color = Color.white;

        Transform header = transform.Find("HeaderArea");
        if (header != null)
        {
            JelliMetaUIRuntimeBuilder.SetTop(header as RectTransform, 0f, 0f, 0f, 108f);
            Image headerBg = JelliMetaUIRuntimeBuilder.EnsureComponent<Image>(header.gameObject);
            headerBg.color = Color.white;
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
            Image bottomBg = JelliMetaUIRuntimeBuilder.EnsureComponent<Image>(bottom.gameObject);
            bottomBg.color = Color.white;
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
            JelliMetaUIRuntimeBuilder.SetStretch(middle as RectTransform, 0f, 0f, 108f, 270f);

        Transform scrollView = transform.Find("MiddleScrollArea/DetailScrollView");
        if (scrollView != null)
            JelliMetaUIRuntimeBuilder.SetStretch(scrollView as RectTransform, 0f, 0f, 0f, 0f);

        Transform content = transform.Find("MiddleScrollArea/DetailScrollView/Viewport/Content");
        if (content != null)
        {
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
            imageLayout.minHeight = 780f;
            imageLayout.preferredHeight = 920f;
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
                JelliMetaUIRuntimeBuilder.Color32(17, 24, 39),
                Color.white,
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

    private void EnsureButtonVisual(Button button, string labelText, Color backgroundColor, Color textColor, float fontSize, TextAlignmentOptions alignment, float leftPadding, float rightPadding, float topPadding, float bottomPadding)
    {
        if (button == null)
            return;

        Graphic backgroundGraphic = ConfigureButtonBackground(button, backgroundColor);
        button.targetGraphic = backgroundGraphic;
        ApplyButtonColorBlock(button, backgroundColor);

        TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label == null)
        {
            Transform existingText = button.transform.Find("Text");
            if (existingText != null)
                existingText.gameObject.SetActive(false);

            label = JelliMetaUIRuntimeBuilder.CreateText(button.transform, "RuntimeButtonLabel", labelText, fontSize, FontStyles.Bold, textColor, alignment);
        }

        label.gameObject.SetActive(true);
        label.transform.SetAsLastSibling();
        JelliMetaUIRuntimeBuilder.ConfigureText(label, labelText, fontSize, FontStyles.Bold, textColor, alignment);
        label.color = textColor;

        RectTransform labelRect = label.rectTransform;
        JelliMetaUIRuntimeBuilder.SetStretch(labelRect, leftPadding, rightPadding, topPadding, bottomPadding);

        LayoutElement labelLayout = label.GetComponent<LayoutElement>();
        if (labelLayout != null)
            Destroy(labelLayout);
    }


    private Graphic ConfigureButtonBackground(Button button, Color backgroundColor)
    {
        if (button == null)
            return null;

        // 기존 Image에 Source Image가 비어 있으면 Scene View에서 빨간 X가 크게 보인다.
        // 버튼 배경은 Sprite 기반 Image 대신 직접 사각형을 그리는 Graphic으로 처리한다.
        Image legacyImage = button.GetComponent<Image>();
        if (legacyImage != null)
            legacyImage.enabled = false;

        if (backgroundColor.a <= 0.01f)
        {
            button.transition = Selectable.Transition.None;
            return null;
        }

        JelliMetaSolidGraphic solidGraphic = button.GetComponent<JelliMetaSolidGraphic>();
        if (solidGraphic == null)
            solidGraphic = button.gameObject.AddComponent<JelliMetaSolidGraphic>();

        solidGraphic.enabled = true;
        solidGraphic.color = backgroundColor;
        solidGraphic.raycastTarget = true;
        return solidGraphic;
    }

    private void ApplyButtonColorBlock(Button button, Color backgroundColor)
    {
        if (button == null)
            return;

        // Unity Button의 ColorTint가 Play 진입 시 targetGraphic.color를 normalColor로 덮어쓴다.
        // 기본 normalColor가 white면 AR 버튼 배경이 흰색으로 돌아가고, 흰색 글자가 안 보인다.
        if (backgroundColor.a <= 0.01f)
        {
            button.transition = Selectable.Transition.None;
            return;
        }

        button.transition = Selectable.Transition.ColorTint;

        ColorBlock colors = button.colors;
        colors.normalColor = backgroundColor;
        colors.selectedColor = backgroundColor;
        colors.highlightedColor = Lighten(backgroundColor, 0.08f);
        colors.pressedColor = Darken(backgroundColor, 0.10f);
        colors.disabledColor = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 0.45f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;
    }

    private Color Lighten(Color color, float amount)
    {
        return new Color(
            Mathf.Clamp01(color.r + amount),
            Mathf.Clamp01(color.g + amount),
            Mathf.Clamp01(color.b + amount),
            color.a
        );
    }

    private Color Darken(Color color, float amount)
    {
        return new Color(
            Mathf.Clamp01(color.r - amount),
            Mathf.Clamp01(color.g - amount),
            Mathf.Clamp01(color.b - amount),
            color.a
        );
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
