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
            rect.sizeDelta = new Vector2(Mathf.Max(170f, (categoryName?.Length ?? 2) * 40f + 80f), 72f);
            rect.localScale = Vector3.one;
        }

        Image background = GetComponent<Image>();
        if (background != null)
            background.color = selected ? JelliMetaUIRuntimeBuilder.Color32(17, 24, 39) : JelliMetaUIRuntimeBuilder.Color32(243, 244, 246);

        if (categoryNameText != null)
        {
            JelliMetaUIRuntimeBuilder.ConfigureText(
                categoryNameText,
                string.IsNullOrEmpty(categoryName) ? "전체" : categoryName,
                30f,
                FontStyles.Bold,
                selected ? Color.white : JelliMetaUIRuntimeBuilder.Color32(17, 24, 39),
                TextAlignmentOptions.Center);

            JelliMetaUIRuntimeBuilder.SetStretch(categoryNameText.rectTransform, 12f, 12f, 0f, 0f);
        }
    }
}
