using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProductCardUI : MonoBehaviour
{
    [Header("UI Components")]
    public Image productImage;
    public TextMeshProUGUI productNameText;
    public TextMeshProUGUI priceText;
    public Button cardButton;

    // 자신이 할당받은 단일 행 데이터
    private ProductData myRowData; 
    private JelliMetaUIManager mainUIManager;

    public void SetupCard(ProductData rowData, JelliMetaUIManager uiManager)
    {
        myRowData = rowData;
        mainUIManager = uiManager;

        if (productImage != null) productImage.sprite = myRowData.productImage;
        if (productNameText != null) productNameText.text = myRowData.productName;
        if (priceText != null) priceText.text = myRowData.price;

        cardButton.onClick.RemoveAllListeners();
        cardButton.onClick.AddListener(() => 
        {
            if (mainUIManager != null)
                mainUIManager.ShowProductDetailScreen(myRowData);
        });
    }
}