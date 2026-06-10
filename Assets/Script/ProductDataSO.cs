using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ProductData
{
    public string productID;
    public string productName;
    public string category;
    public string price;

    [TextArea(3, 5)]
    public string description;

    public string dimensions;
    public string material;

    public Sprite productImage;
    public GameObject arModelPrefab;
}

[CreateAssetMenu(fileName = "MasterProductDatabase", menuName = "JelliMeta/Master Product Database")]
public class ProductDataSO : ScriptableObject
{
    public List<ProductData> productList = new List<ProductData>();
}
