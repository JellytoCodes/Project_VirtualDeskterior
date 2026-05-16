using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CategoryButtonUI : MonoBehaviour
{
    public TextMeshProUGUI categoryNameText;
    public Button categoryButton;

    public void SetupCategory(string name)
    {
        if (categoryNameText != null) categoryNameText.text = name;
        
        if (categoryButton != null)
        {
            categoryButton.onClick.RemoveAllListeners();
            categoryButton.onClick.AddListener(() => Debug.Log($"{name} 필터 클릭됨"));
        }
    }
}