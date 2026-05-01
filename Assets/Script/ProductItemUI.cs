using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class ProductData
{
    public string productName;
    public string description;
    public Texture2D productImage;
    public GameObject modelPrefab;
    public bool isAvailable;
}

public class ProductItemUI : MonoBehaviour
{
    public RawImage itemImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public Button previewButton;
    public Button buyButton;

    public void Setup(ProductData data, VirtualDeskteriorManager manager)
    {
        if (data.isAvailable)
        {
            nameText.text = data.productName;
            descText.text = data.description;
            
            if (data.productImage != null)
            {
                itemImage.texture = data.productImage;
            }

            previewButton.interactable = true;
            buyButton.interactable = true;

            previewButton.onClick.AddListener(() => manager.OnClickPlacementButton(data.modelPrefab));
        }
        else
        {
            nameText.text = "준비 중인 상품";
            descText.text = "곧 업데이트 될 예정입니다.";
            
            previewButton.interactable = false;
            buyButton.interactable = false;
        }
    }
}