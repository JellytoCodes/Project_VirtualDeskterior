using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class BannerData
{
    public string bannerTitle;
    public Sprite bannerImage; 
}

public class HomeUIManager : MonoBehaviour
{
    [Header("System Managers")]
    public JelliMetaUIManager mainUIManager;

    [Header("1. Hero Banner Setup")]
    public BannerData currentBannerData;     
    public Image bannerThumbnailImage;       
    public TextMeshProUGUI bannerTitleText;  
    public Button bannerDetailButton;        

    [Header("2. Category Menu Setup")]
    public Transform categoryContentParent;  
    public GameObject categoryPillPrefab;    
    public List<string> categoryList = new List<string>() { "전체", "책상", "의자", "모니터", "조명", "소품" };

    [Header("3. Product Grid Setup")]
    public Transform productGridParent;      
    public GameObject productCardPrefab;     
    
    // ★ 변경점: 리스트가 아니라 단일 마스터 DB 에셋 하나만 꽂습니다.
    public ProductDatabaseSO masterDatabase;

    private void Start()
    {
        SetupHeroBanner();
        GenerateCategoryMenu();
        GenerateProductGrid();
    }

    private void SetupHeroBanner()
    {
        if (bannerThumbnailImage != null && currentBannerData.bannerImage != null)
            bannerThumbnailImage.sprite = currentBannerData.bannerImage;
        
        if (bannerTitleText != null) bannerTitleText.text = currentBannerData.bannerTitle;

        if (bannerDetailButton != null)
        {
            bannerDetailButton.onClick.RemoveAllListeners();
            bannerDetailButton.onClick.AddListener(() => Debug.Log("배너 클릭됨"));
        }
    }

    private void GenerateCategoryMenu()
    {
        foreach (Transform child in categoryContentParent) Destroy(child.gameObject);

        foreach (string categoryName in categoryList)
        {
            GameObject newCategoryObj = Instantiate(categoryPillPrefab, categoryContentParent);
            CategoryButtonUI categoryUI = newCategoryObj.GetComponent<CategoryButtonUI>();
            if (categoryUI != null) categoryUI.SetupCategory(categoryName);
        }
    }

    public void GenerateProductGrid()
    {
        foreach (Transform child in productGridParent) Destroy(child.gameObject);

        if (masterDatabase == null)
        {
            Debug.LogError("[HomeUIManager] 마스터 DB가 연결되지 않았습니다!");
            return;
        }

        // ★ 변경점: 마스터 DB 안의 Array를 for each 루프로 순회
        foreach (ProductData rowData in masterDatabase.productList)
        {
            GameObject newCard = Instantiate(productCardPrefab, productGridParent);
            ProductCardUI cardUI = newCard.GetComponent<ProductCardUI>();

            if (cardUI != null)
            {
                // 구조체(Row Data) 하나를 통째로 카드에 던져줍니다.
                cardUI.SetupCard(rowData, mainUIManager);
            }
        }
    }
}