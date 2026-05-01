using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AR;
using UnityEngine.XR.ARFoundation;

public class VirtualDeskteriorManager : MonoBehaviour
{
    public GameObject shoppingUIPanel;
    public GameObject arUIPanel;
    public ARPlacementInteractable placementInteractable;
    
    public ARCameraManager arCameraManager; 
    public Camera mainCamera;

    private void Start()
    {
        placementInteractable.enabled = false;
        arUIPanel.SetActive(false);

        if (arCameraManager != null)
        {
            arCameraManager.enabled = false;
        }
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = Color.white;

        Screen.fullScreen = false;
    }

    public void OnClickPlacementButton(GameObject modelPrefab)
    {
        placementInteractable.placementPrefab = modelPrefab;
        shoppingUIPanel.SetActive(false);
        arUIPanel.SetActive(true);
        placementInteractable.enabled = true;

        mainCamera.clearFlags = CameraClearFlags.Color;
        if (arCameraManager != null)
        {
            arCameraManager.enabled = true;
        }

        Screen.fullScreen = true;
    }

    public void ReturnToShoppingUI()
    {
        placementInteractable.enabled = false;
        placementInteractable.placementPrefab = null;
        arUIPanel.SetActive(false);
        shoppingUIPanel.SetActive(true);

        if (arCameraManager != null)
        {
            arCameraManager.enabled = false;
        }
        mainCamera.clearFlags = CameraClearFlags.SolidColor;

        Screen.fullScreen = false;
    }
}