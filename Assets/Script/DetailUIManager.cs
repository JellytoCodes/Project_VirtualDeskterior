using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DetailUIManager : MonoBehaviour
{
    [Header("System Managers")]
    public JelliMetaUIManager mainUIManager; // HomeUIManager처럼 얘만 참조함

    [Header("Detail UI Elements")]
    public Image detailProductImage;
    public TextMeshProUGUI detailProductNameText;
    public TextMeshProUGUI detailPriceText;
    public TextMeshProUGUI detailDescriptionText;

    [Header("Detail Buttons")]
    public Button backButton;
    public Button openARButton;

    // ★ 핵심: 이 패널 게임오브젝트가 켜질 때(SetActive=true)마다 자동으로 실행됨
    private void OnEnable()
    {
        if (mainUIManager != null)
        {
            // 최상위 매니저가 들고 있는 현재 데이터를 쏙 빼와서 세팅함
            SetupDetailPanel(mainUIManager.currentSelectedProduct);
        }
    }

    private void SetupDetailPanel(ProductData data)
    {
        if (detailProductImage != null) detailProductImage.sprite = data.productImage;
        if (detailProductNameText != null) detailProductNameText.text = data.productName;
        if (detailPriceText != null) detailPriceText.text = data.price;
        if (detailDescriptionText != null) detailDescriptionText.text = data.description;

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
                Debug.Log($"[AR 진입] {data.productName} 모델링 로드");
            });
        }
    }
}