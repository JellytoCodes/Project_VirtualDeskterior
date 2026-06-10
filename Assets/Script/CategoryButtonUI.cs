using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CategoryButtonUI : MonoBehaviour
{
    public TextMeshProUGUI categoryNameText;
    public Button categoryButton;

    private string categoryName;
    private Action<string> clickCallback;

    public void SetupCategory(string name)
    {
        SetupCategory(name, null, false);
    }

    public void SetupCategory(string name, Action<string> onClick, bool selected)
    {
        categoryName = name;
        clickCallback = onClick;

        if (categoryButton == null)
            categoryButton = GetComponent<Button>();

        if (categoryNameText == null)
            categoryNameText = GetComponentInChildren<TextMeshProUGUI>(true);

        NormalizeVisual(selected);

        if (categoryButton != null)
        {
            categoryButton.onClick.RemoveAllListeners();
            categoryButton.onClick.AddListener(() =>
            {
                Debug.Log($"[CategoryButtonUI] {categoryName} 필터 클릭됨");
                clickCallback?.Invoke(categoryName);
            });
        }
    }

    public void SetSelected(bool selected)
    {
        NormalizeVisual(selected);
    }

    private void NormalizeVisual(bool selected)
    {
        RectTransform rect = transform as RectTransform;
        if (rect != null)
        {
            float width = Mathf.Clamp((categoryName?.Length ?? 2) * 34f + 96f, 132f, 230f);
            rect.sizeDelta = new Vector2(width, 76f);
            rect.localScale = Vector3.one;
        }

        if (categoryButton == null)
            categoryButton = GetComponent<Button>();

        Color backgroundColor = selected ? JelliMetaUIRuntimeBuilder.Color32(17, 24, 39) : JelliMetaUIRuntimeBuilder.Color32(243, 244, 246);
        Color textColor = selected ? Color.white : JelliMetaUIRuntimeBuilder.Color32(17, 24, 39);

        if (categoryButton != null)
        {
            JelliMetaUIRuntimeBuilder.MakeImageTransparentButRaycastable(categoryButton.gameObject);
            categoryButton.transition = Selectable.Transition.None;
            categoryButton.interactable = true;
        }

        JelliMetaUIRuntimeBuilder.EnsureRoundedBackground(transform, "RuntimePillBackground", backgroundColor, 38f);

        if (categoryNameText == null)
            categoryNameText = GetComponentInChildren<TextMeshProUGUI>(true);

        if (categoryNameText == null)
            categoryNameText = JelliMetaUIRuntimeBuilder.CreateText(transform, "CategoryNameText", "", 30f, FontStyles.Bold, textColor, TextAlignmentOptions.Center);

        categoryNameText.gameObject.SetActive(true);
        categoryNameText.transform.SetAsLastSibling();
        JelliMetaUIRuntimeBuilder.ConfigureText(
            categoryNameText,
            string.IsNullOrEmpty(categoryName) ? "전체" : categoryName,
            30f,
            FontStyles.Bold,
            textColor,
            TextAlignmentOptions.Center);

        JelliMetaUIRuntimeBuilder.SetStretch(categoryNameText.rectTransform, 18f, 18f, 0f, 0f);
        categoryNameText.textWrappingMode = TextWrappingModes.NoWrap;
        categoryNameText.overflowMode = TextOverflowModes.Overflow;
        categoryNameText.raycastTarget = false;
    }
}
