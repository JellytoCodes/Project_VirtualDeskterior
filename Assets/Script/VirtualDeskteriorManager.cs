using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class VirtualDeskteriorManager : MonoBehaviour
{
    public GameObject shoppingUIPanel;
    public GameObject arUIPanel;
    public Camera mainCamera;

    private ARSession arSession; 
    private GameObject currentSpawnedObject;

    private void Start()
    {
        arSession = FindFirstObjectByType<ARSession>();

        arUIPanel.SetActive(false);
        shoppingUIPanel.SetActive(true);

        if (arSession != null)
        {
            arSession.enabled = false;
        }
    }

    public void OnClickPlacementButton(GameObject modelPrefab, float spawnScale)
    {
        shoppingUIPanel.SetActive(false);
        arUIPanel.SetActive(true);

        if (arSession != null)
        {
            arSession.enabled = true;
        }

        if (currentSpawnedObject == null)
        {
            Vector3 spawnPos = mainCamera.transform.position + mainCamera.transform.forward * 1.0f;
            spawnPos.y -= 0.2f; 
            
            Quaternion spawnRot = Quaternion.LookRotation(mainCamera.transform.position - spawnPos);
            spawnRot.x = 0; spawnRot.z = 0; 

            currentSpawnedObject = Instantiate(modelPrefab, spawnPos, spawnRot);
            
            currentSpawnedObject.transform.localScale = Vector3.one * spawnScale;
        }
    }

    public void ReturnToShoppingUI()
    {
        arUIPanel.SetActive(false);
        shoppingUIPanel.SetActive(true);

        if (arSession != null)
        {
            arSession.enabled = false;
        }

        if (currentSpawnedObject != null)
        {
            Destroy(currentSpawnedObject);
            currentSpawnedObject = null;
        }
    }
}