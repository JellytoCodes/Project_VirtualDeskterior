using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TextCore.LowLevel;

public static class JelliMetaUIRuntimeBuilder
{
    private static TMP_FontAsset boldFont;
    private static Sprite solidSprite;

    private const string BoldTtfResourcePath = "Font/GmarketSansTTFBold";
    private const string BrokenPrebakedSdfResourcePath = "Font/GmarketSansTTFBold SDF";

    private const string KoreanPreloadCharacters =
        "가구소품검색미니멀데스크테리어자세히보기전체책상의자모니터조명수납홈주문내역찜마이페이지" +
        "화이트체어가격정보없음상품설명등록공간에서보기장바구니담기크기소재뒤로리셋캡처편집완료" +
        "일이삼사오육칠팔구십영만천백원ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 .,!?()/-_";

    public static TMP_FontAsset RegularFont
    {
        get
        {
            // 한글 표시 안정성을 위해 모든 TMP Text를 Bold SDF 하나로 통일한다.
            return BoldFont;
        }
    }

    public static TMP_FontAsset BoldFont
    {
        get
        {
            if (boldFont == null)
                boldFont = CreateDynamicKoreanFontAsset();
            return boldFont;
        }
    }

    private static TMP_FontAsset CreateDynamicKoreanFontAsset()
    {
        Font sourceFont = Resources.Load<Font>(BoldTtfResourcePath);
        TMP_FontAsset fontAsset = null;

        if (sourceFont != null)
        {
            fontAsset = TMP_FontAsset.CreateFontAsset(
                sourceFont,
                90,
                9,
                GlyphRenderMode.SDFAA,
                2048,
                2048,
                AtlasPopulationMode.Dynamic,
                true
            );

            fontAsset.name = "JelliMeta_GmarketSansTTFBold_Dynamic_TMP";
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            fontAsset.isMultiAtlasTexturesEnabled = true;
            fontAsset.hideFlags = HideFlags.HideAndDontSave;
        }

        if (fontAsset == null)
            fontAsset = Resources.Load<TMP_FontAsset>(BrokenPrebakedSdfResourcePath);

        if (fontAsset != null)
        {
            fontAsset.TryAddCharacters(KoreanPreloadCharacters, out string _);
            TMP_Settings.defaultFontAsset = fontAsset;
        }

        return fontAsset;
    }

    public static Sprite SolidSprite
    {
        get
        {
            if (solidSprite == null)
            {
                Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                texture.name = "JelliMeta_RuntimeSolidWhite";
                texture.hideFlags = HideFlags.HideAndDontSave;
                texture.SetPixel(0, 0, Color.white);
                texture.Apply();

                solidSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 100f);
                solidSprite.name = "JelliMeta_RuntimeSolidSprite";
                solidSprite.hideFlags = HideFlags.HideAndDontSave;
            }

            return solidSprite;
        }
    }

    public static void ApplyProjectFonts(GameObject root)
    {
        if (root == null)
            return;

        TMP_FontAsset font = BoldFont;
        if (font == null)
            return;

        foreach (TextMeshProUGUI text in root.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            text.font = font;
            PreloadTextCharacters(text.text);
            text.raycastTarget = false;
            text.SetAllDirty();
        }

        foreach (TMP_InputField inputField in root.GetComponentsInChildren<TMP_InputField>(true))
        {
            if (inputField.textComponent != null)
            {
                inputField.textComponent.font = font;
                PreloadTextCharacters(inputField.textComponent.text);
                inputField.textComponent.SetAllDirty();
            }

            if (inputField.placeholder is TextMeshProUGUI placeholder)
            {
                placeholder.font = font;
                PreloadTextCharacters(placeholder.text);
                placeholder.SetAllDirty();
            }
        }

        // 주의: 여기서 Image를 일괄 수정하면 상품 이미지/배너 이미지까지 1x1 스프라이트로 덮여
        // 화면 중앙에 거대한 흐릿한 사각형처럼 보일 수 있다. 이미지 일괄 보정은 하지 않는다.
    }

    public static void ConfigureCanvasForMobile(GameObject root)
    {
        if (root == null)
            return;

        CanvasScaler scaler = root.GetComponentInParent<CanvasScaler>();
        if (scaler == null)
            scaler = Object.FindAnyObjectByType<CanvasScaler>();

        if (scaler == null)
            return;

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }

    public static void SetStretch(RectTransform rect, float left, float right, float top, float bottom)
    {
        if (rect == null)
            return;

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
        rect.localScale = Vector3.one;
    }

    public static void SetTop(RectTransform rect, float left, float right, float top, float height)
    {
        if (rect == null)
            return;

        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.offsetMin = new Vector2(left, -top - height);
        rect.offsetMax = new Vector2(-right, -top);
        rect.localScale = Vector3.one;
    }

    public static void SetBottom(RectTransform rect, float left, float right, float bottom, float height)
    {
        if (rect == null)
            return;

        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, bottom + height);
        rect.localScale = Vector3.one;
    }

    public static void SetFixed(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        if (rect == null)
            return;

        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
    }

    public static T EnsureComponent<T>(GameObject target) where T : Component
    {
        if (target == null)
            return null;

        T component = target.GetComponent<T>();
        if (component == null)
            component = target.AddComponent<T>();

        // 새로 만들거나 배경용으로 쓰는 Image는 Source Image가 비어 있으면 Scene View에서 빨간 X처럼 보일 수 있다.
        // 단, 이 함수로 직접 접근한 배경 Image에만 1x1 sprite를 넣는다. 전체 Image 일괄 수정은 금지.
        if (component is Image image && image.sprite == null)
        {
            image.sprite = SolidSprite;
            image.type = Image.Type.Simple;
        }

        return component;
    }

    public static TextMeshProUGUI CreateText(Transform parent, string name, string text, float fontSize, FontStyles style, Color color, TextAlignmentOptions alignment)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI label = go.GetComponent<TextMeshProUGUI>();
        ConfigureText(label, text, fontSize, style, color, alignment);
        return label;
    }

    public static void ConfigureText(TextMeshProUGUI label, string text, float fontSize, FontStyles style, Color color, TextAlignmentOptions alignment)
    {
        if (label == null)
            return;

        label.text = text;
        label.font = BoldFont;
        PreloadTextCharacters(text);
        label.fontSize = fontSize;
        label.fontStyle = style;
        label.color = color;
        label.alignment = alignment;
        label.textWrappingMode = TextWrappingModes.Normal;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.raycastTarget = false;
    }


    public static JelliMetaRoundedRectGraphic EnsureRoundedBackground(Transform parent, string name, Color color, float radius)
    {
        if (parent == null)
            return null;

        Transform existing = parent.Find(name);
        JelliMetaRoundedRectGraphic graphic = null;

        if (existing == null)
        {
            GameObject background = new GameObject(name, typeof(RectTransform), typeof(JelliMetaRoundedRectGraphic));
            background.transform.SetParent(parent, false);
            background.transform.SetAsFirstSibling();
            existing = background.transform;
        }
        else
        {
            graphic = existing.GetComponent<JelliMetaRoundedRectGraphic>();
            existing.SetAsFirstSibling();
        }

        RectTransform rect = existing as RectTransform;
        SetStretch(rect, 0f, 0f, 0f, 0f);

        if (graphic == null)
            graphic = existing.GetComponent<JelliMetaRoundedRectGraphic>();

        if (graphic != null)
        {
            graphic.color = color;
            graphic.radius = radius;
            graphic.raycastTarget = false;
            graphic.SetAllDirty();
        }

        return graphic;
    }

    public static void MakeImageTransparentButRaycastable(GameObject target)
    {
        if (target == null)
            return;

        Image image = target.GetComponent<Image>();
        if (image == null)
            image = target.AddComponent<Image>();

        if (image.sprite == null)
        {
            image.sprite = SolidSprite;
            image.type = Image.Type.Simple;
        }

        image.color = new Color(1f, 1f, 1f, 0.001f);
        image.raycastTarget = true;
    }

    public static void ConfigureRoundedButton(Button button, string labelText, Color background, Color textColor, float fontSize, float radius)
    {
        if (button == null)
            return;

        MakeImageTransparentButRaycastable(button.gameObject);
        EnsureRoundedBackground(button.transform, "RuntimeRoundedButtonBackground", background, radius);

        button.transition = Selectable.Transition.None;
        button.interactable = true;

        TextMeshProUGUI label = button.transform.Find("RuntimeButtonLabel")?.GetComponent<TextMeshProUGUI>();
        if (label == null)
            label = CreateText(button.transform, "RuntimeButtonLabel", labelText, fontSize, FontStyles.Bold, textColor, TextAlignmentOptions.Center);

        foreach (TextMeshProUGUI other in button.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (other != label)
                other.gameObject.SetActive(false);
        }

        label.gameObject.SetActive(true);
        label.transform.SetAsLastSibling();
        SetStretch(label.rectTransform, 16f, 16f, 0f, 0f);
        ConfigureText(label, labelText, fontSize, FontStyles.Bold, textColor, TextAlignmentOptions.Center);
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Overflow;
        label.raycastTarget = false;
    }

    public static Button CreateButton(Transform parent, string name, string labelText, Color background, Color textColor, float fontSize)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        Image image = go.GetComponent<Image>();
        image.sprite = SolidSprite;
        image.type = Image.Type.Simple;
        image.color = new Color(1f, 1f, 1f, 0.001f);
        image.raycastTarget = true;

        Button button = go.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;

        EnsureRoundedBackground(go.transform, "RuntimeRoundedButtonBackground", background, 32f);
        TextMeshProUGUI label = CreateText(go.transform, "Label", labelText, fontSize, FontStyles.Bold, textColor, TextAlignmentOptions.Center);
        label.transform.SetAsLastSibling();
        SetStretch(label.rectTransform, 8f, 8f, 0f, 0f);
        return button;
    }

    private static void PreloadTextCharacters(string text)
    {
        if (string.IsNullOrEmpty(text) || boldFont == null)
            return;

        boldFont.TryAddCharacters(text, out string _);
    }

    public static Color Color32(int r, int g, int b, int a = 255)
    {
        return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }
}
