using System.Collections.Generic;
using UnityEngine;

public class ShoppingUIManager : MonoBehaviour
{
    public VirtualDeskteriorManager deskteriorManager;
    public GameObject productItemPrefab;
    public Transform scrollViewContent;

    public List<ProductData> productDatabase = new List<ProductData>();

    void Start()
    {
        GenerateProductList();
    }

    void GenerateProductList()
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject newItem = Instantiate(productItemPrefab, scrollViewContent);
            ProductItemUI itemUI = newItem.GetComponent<ProductItemUI>();

            if (i < 3 && i < productDatabase.Count)
            {
                itemUI.Setup(productDatabase[i], deskteriorManager);
            }
            else
            {
                ProductData dummyData = new ProductData();
                dummyData.isAvailable = false;
                itemUI.Setup(dummyData, deskteriorManager);
            }
        }
    }
}