using UnityEngine;

public class JelliMetaUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject homePanel;
    public GameObject detailPanel;
    public GameObject arUIPanel;
    public GameObject bottomNavBar; 

    // 외부에서 데이터를 가져갈 수 있게 public (또는 프로퍼티)으로 열어둠
    [HideInInspector] 
    public ProductData currentSelectedProduct;

    private void Start()
    {
        ShowHomePanel();
    }

    public void ShowHomePanel()
    {
        if (homePanel) homePanel.SetActive(true);
        if (bottomNavBar) bottomNavBar.SetActive(true);
        
        if (detailPanel) detailPanel.SetActive(false);
        if (arUIPanel) arUIPanel.SetActive(false);
    }

    // 카드가 클릭되면 호출됨
    public void ShowProductDetailScreen(ProductData rowData)
    {
        // 1. 현재 선택된 데이터를 중앙에 갱신
        currentSelectedProduct = rowData; 

        // 2. 패널 끄고 켜기만 함 (DetailUIManager 호출 안 함!)
        if (homePanel) homePanel.SetActive(false);
        if (bottomNavBar) bottomNavBar.SetActive(false); 
        
        if (detailPanel) detailPanel.SetActive(true);
    }
}