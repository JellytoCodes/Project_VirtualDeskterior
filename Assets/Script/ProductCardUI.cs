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

    private ProductData myRowData;
    private JelliMetaUIManager mainUIManager;

    public void SetupCard(ProductData rowData, JelliMetaUIManager uiManager)
    {
        myRowData = rowData;
        mainUIManager = uiManager;

        ResolveReferences();
        NormalizeCardVisual();

        if (productImage != null)
        {
            productImage.sprite = myRowData.productImage;
            productImage.preserveAspect = true;
            productImage.color = Color.white;
        }

        if (productNameText != null)
            JelliMetaUIRuntimeBuilder.ConfigureText(productNameText, myRowData.productName, 34f, FontStyles.Bold, Color.black, TextAlignmentOptions.Left);

        if (priceText != null)
            JelliMetaUIRuntimeBuilder.ConfigureText(priceText, myRowData.price, 30f, FontStyles.Normal, JelliMetaUIRuntimeBuilder.Color32(107, 114, 128), TextAlignmentOptions.Left);

        if (cardButton != null)
        {
            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(() =>
            {
                if (mainUIManager != null)
                    mainUIManager.ShowProductDetailScreen(myRowData);
            });
        }
    }

    private void ResolveReferences()
    {
        if (cardButton == null)
            cardButton = GetComponent<Button>();

        if (productImage == null)
        {
            Transform imageTransform = transform.Find("ProductImage");
            if (imageTransform != null)
                productImage = imageTransform.GetComponent<Image>();
        }

        if (productNameText == null)
        {
            Transform nameTransform = transform.Find("ProductNameText");
            if (nameTransform != null)
                productNameText = nameTransform.GetComponent<TextMeshProUGUI>();
        }

        if (priceText == null)
        {
            Transform priceTransform = transform.Find("PriceText");
            if (priceTransform != null)
                priceText = priceTransform.GetComponent<TextMeshProUGUI>();
        }
    }

    private void NormalizeCardVisual()
    {
        RectTransform root = transform as RectTransform;
        if (root != null)
        {
            root.pivot = new Vector2(0.5f, 1f);
            root.localScale = Vector3.one;
        }

        Image background = GetComponent<Image>();
        if (background != null)
            background.color = new Color(1f, 1f, 1f, 0f);

        if (productImage != null)
        {
            RectTransform imageRect = productImage.rectTransform;
            imageRect.anchorMin = new Vector2(0f, 1f);
            imageRect.anchorMax = new Vector2(1f, 1f);
            imageRect.pivot = new Vector2(0.5f, 1f);
            imageRect.anchoredPosition = Vector2.zero;
            imageRect.sizeDelta = new Vector2(0f, -150f);
            imageRect.localScale = Vector3.one;

            productImage.type = Image.Type.Simple;
            productImage.preserveAspect = true;
        }

        if (productNameText != null)
        {
            RectTransform nameRect = productNameText.rectTransform;
            nameRect.anchorMin = new Vector2(0f, 0f);
            nameRect.anchorMax = new Vector2(1f, 0f);
            nameRect.pivot = new Vector2(0.5f, 0f);
            nameRect.anchoredPosition = new Vector2(0f, 72f);
            nameRect.sizeDelta = new Vector2(0f, 64f);
            nameRect.localScale = Vector3.one;
        }

        if (priceText != null)
        {
            RectTransform priceRect = priceText.rectTransform;
            priceRect.anchorMin = new Vector2(0f, 0f);
            priceRect.anchorMax = new Vector2(1f, 0f);
            priceRect.pivot = new Vector2(0.5f, 0f);
            priceRect.anchoredPosition = new Vector2(0f, 18f);
            priceRect.sizeDelta = new Vector2(0f, 48f);
            priceRect.localScale = Vector3.one;
        }
    }
}
