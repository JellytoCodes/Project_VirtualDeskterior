using System.Collections.Generic;
using UnityEngine;

// 언리얼의 DataTable Row(행) 역할
[System.Serializable]
public struct ProductData
{
    public string productID;         // 고유 ID (관리용)
    public string productName;
    public string price;
    
    [TextArea(3, 5)] 
    public string description; 
    
    public Sprite productImage;
    public GameObject arModelPrefab;
}

// 언리얼의 UDataTable (마스터 DB) 역할
[CreateAssetMenu(fileName = "MasterProductDatabase", menuName = "JelliMeta/Master Product Database")]
public class ProductDatabaseSO : ScriptableObject
{
    // 여기에 모든 상품 데이터가 Array(List) 형태로 들어갑니다.
    public List<ProductData> productList = new List<ProductData>();
}