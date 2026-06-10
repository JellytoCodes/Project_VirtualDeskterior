using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JelliMetaUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject homePanel;
    public GameObject detailPanel;
    public GameObject arUIPanel;
    public GameObject bottomNavBar;

    [Header("AR / Preview")]
    public ARInteractionManager arInteractionManager;
    public Camera mainCamera;
    public float defaultSpawnScale = 0.3f;

    [HideInInspector]
    public ProductData currentSelectedProduct;

    private const string RuntimeAROverlayName = "RuntimeARPreviewOverlay";
    private ProductData activeARProduct;

    private void Start()
    {
        JelliMetaUIRuntimeBuilder.ConfigureCanvasForMobile(gameObject);
        EnsureARInteractionManager();
        NormalizeBottomNavigation();
        ShowHomePanel();
    }

    private void EnsureARInteractionManager()
    {
        if (arInteractionManager == null)
            arInteractionManager = FindAnyObjectByType<ARInteractionManager>();

        if (arInteractionManager == null)
        {
            GameObject managerObject = new GameObject("ARInteractionManager_Runtime");
            arInteractionManager = managerObject.AddComponent<ARInteractionManager>();
            Debug.Log("[JelliMetaUIManager] ARInteractionManager가 없어 런타임에 자동 생성했습니다.");
        }

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            mainCamera = FindAnyObjectByType<Camera>();

        if (arInteractionManager.mainCamera == null)
            arInteractionManager.mainCamera = mainCamera;
    }

    public void ShowHomePanel()
    {
        if (arInteractionManager != null)
            arInteractionManager.StopARSession();

        if (homePanel) homePanel.SetActive(true);
        if (bottomNavBar) bottomNavBar.SetActive(true);

        if (detailPanel) detailPanel.SetActive(false);
        if (arUIPanel) arUIPanel.SetActive(false);
    }

    public void ShowProductDetailScreen(ProductData rowData)
    {
        currentSelectedProduct = rowData;

        if (homePanel) homePanel.SetActive(false);
        if (bottomNavBar) bottomNavBar.SetActive(false);

        if (detailPanel) detailPanel.SetActive(true);
        if (arUIPanel) arUIPanel.SetActive(false);
    }

    public void ShowARPlacementScreen()
    {
        ShowARPlacementScreen(currentSelectedProduct);
    }

    public void ShowARPlacementScreen(ProductData rowData)
    {
        currentSelectedProduct = rowData;
        activeARProduct = rowData;
        EnsureARInteractionManager();

        if (homePanel) homePanel.SetActive(false);
        if (detailPanel) detailPanel.SetActive(false);
        if (bottomNavBar) bottomNavBar.SetActive(false);
        if (arUIPanel) arUIPanel.SetActive(true);

        EnsureARPreviewOverlay(rowData);

        if (arInteractionManager == null)
        {
            Debug.LogError("[JelliMetaUIManager] ARInteractionManager를 찾거나 생성하지 못했습니다.");
            return;
        }

        Debug.Log($"[JelliMetaUIManager] AR Preview 진입: {rowData.productName}");
        arInteractionManager.StartARPlacement(rowData.arModelPrefab, defaultSpawnScale);
    }

    private void NormalizeBottomNavigation()
    {
        if (bottomNavBar == null)
            return;

        RectTransform rect = bottomNavBar.transform as RectTransform;
        JelliMetaUIRuntimeBuilder.SetBottom(rect, 0f, 0f, 0f, 144f);

        Image bg = JelliMetaUIRuntimeBuilder.EnsureComponent<Image>(bottomNavBar);
        bg.color = Color.white;

        Transform existing = bottomNavBar.transform.Find("RuntimeBottomNavigation");
        if (existing == null)
        {
            GameObject container = new GameObject("RuntimeBottomNavigation", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            container.transform.SetParent(bottomNavBar.transform, false);
            JelliMetaUIRuntimeBuilder.SetStretch(container.transform as RectTransform, 56f, 56f, 10f, 14f);

            HorizontalLayoutGroup group = container.GetComponent<HorizontalLayoutGroup>();
            group.childAlignment = TextAnchor.MiddleCenter;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = true;
            group.spacing = 20f;

            CreateNavItem(container.transform, "홈", true);
            CreateNavItem(container.transform, "주문내역", false);
            CreateNavItem(container.transform, "찜", false);
            CreateNavItem(container.transform, "마이페이지", false);
        }

        JelliMetaUIRuntimeBuilder.ApplyProjectFonts(bottomNavBar);
    }

    private void CreateNavItem(Transform parent, string text, bool selected)
    {
        GameObject item = new GameObject(text, typeof(RectTransform));
        item.transform.SetParent(parent, false);
        TextMeshProUGUI label = JelliMetaUIRuntimeBuilder.CreateText(item.transform, "Label", text, 26f, selected ? FontStyles.Bold : FontStyles.Normal,
            selected ? JelliMetaUIRuntimeBuilder.Color32(17, 24, 39) : JelliMetaUIRuntimeBuilder.Color32(156, 163, 175), TextAlignmentOptions.Center);
        JelliMetaUIRuntimeBuilder.SetStretch(label.rectTransform, 0f, 0f, 0f, 0f);
    }

    private void EnsureARPreviewOverlay(ProductData rowData)
    {
        if (arUIPanel == null)
            return;

        RectTransform panelRect = arUIPanel.transform as RectTransform;
        JelliMetaUIRuntimeBuilder.SetStretch(panelRect, 0f, 0f, 0f, 0f);

        // AR 화면에서는 카메라 렌더링이 뒤에 보여야 하므로
        // ARUIPanel 자체의 Image 배경은 끈다.
        DisablePanelBlockingImage(arUIPanel);

        Transform existing = arUIPanel.transform.Find(RuntimeAROverlayName);
        if (existing == null)
            existing = BuildAROverlay();

        RemoveCameraGuide(existing);
        UpdateAROverlay(existing, rowData);
        JelliMetaUIRuntimeBuilder.ApplyProjectFonts(arUIPanel);
    }

    private Transform BuildAROverlay()
    {
        GameObject overlay = new GameObject(RuntimeAROverlayName, typeof(RectTransform));
        overlay.transform.SetParent(arUIPanel.transform, false);
        JelliMetaUIRuntimeBuilder.SetStretch(overlay.transform as RectTransform, 0f, 0f, 0f, 0f);
        SetUILayerRecursive(overlay, arUIPanel.layer);

        Button backButton = JelliMetaUIRuntimeBuilder.CreateButton(overlay.transform, "BackButton", "뒤로", new Color(1f, 1f, 1f, 0.94f), JelliMetaUIRuntimeBuilder.Color32(17, 24, 39), 25f);
        JelliMetaUIRuntimeBuilder.SetFixed(backButton.transform as RectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(44f, -44f), new Vector2(116f, 88f));
        JelliMetaUIRuntimeBuilder.EnsureRoundedBackground(backButton.transform, "RuntimeRoundedButtonBackground", new Color(1f, 1f, 1f, 0.94f), 44f);
        backButton.onClick.AddListener(() => ShowProductDetailScreen(activeARProduct));

        Button resetButton = JelliMetaUIRuntimeBuilder.CreateButton(overlay.transform, "ResetButton", "리셋", new Color(1f, 1f, 1f, 0.94f), JelliMetaUIRuntimeBuilder.Color32(17, 24, 39), 25f);
        JelliMetaUIRuntimeBuilder.SetFixed(resetButton.transform as RectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-44f, -44f), new Vector2(116f, 88f));
        JelliMetaUIRuntimeBuilder.EnsureRoundedBackground(resetButton.transform, "RuntimeRoundedButtonBackground", new Color(1f, 1f, 1f, 0.94f), 44f);
        resetButton.onClick.AddListener(() =>
        {
            if (arInteractionManager != null)
                arInteractionManager.StartARPlacement(activeARProduct.arModelPrefab, defaultSpawnScale);
        });

        GameObject sheet = new GameObject("BottomSheet", typeof(RectTransform), typeof(Image));
        sheet.transform.SetParent(overlay.transform, false);
        JelliMetaUIRuntimeBuilder.SetBottom(sheet.transform as RectTransform, 44f, 44f, 56f, 248f);
        Image sheetBg = sheet.GetComponent<Image>();
        sheetBg.sprite = JelliMetaUIRuntimeBuilder.SolidSprite;
        sheetBg.color = new Color(1f, 1f, 1f, 0.001f);
        sheetBg.raycastTarget = true;
        JelliMetaUIRuntimeBuilder.EnsureRoundedBackground(sheet.transform, "RuntimeBottomSheetRoundedBackground", Color.white, 52f);

        Image thumb = new GameObject("Thumbnail", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        thumb.transform.SetParent(sheet.transform, false);
        JelliMetaUIRuntimeBuilder.SetFixed(thumb.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -40f), new Vector2(144f, 144f));
        thumb.color = JelliMetaUIRuntimeBuilder.Color32(249, 250, 251);
        thumb.preserveAspect = false;

        TextMeshProUGUI productName = JelliMetaUIRuntimeBuilder.CreateText(sheet.transform, "ProductName", "", 34f, FontStyles.Bold, Color.black, TextAlignmentOptions.Left);
        JelliMetaUIRuntimeBuilder.SetFixed(productName.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(216f, -42f), new Vector2(500f, 84f));

        TextMeshProUGUI productPrice = JelliMetaUIRuntimeBuilder.CreateText(sheet.transform, "ProductPrice", "", 30f, FontStyles.Normal, JelliMetaUIRuntimeBuilder.Color32(107, 114, 128), TextAlignmentOptions.Left);
        JelliMetaUIRuntimeBuilder.SetFixed(productPrice.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(216f, -122f), new Vector2(500f, 48f));

        Button captureButton = JelliMetaUIRuntimeBuilder.CreateButton(sheet.transform, "CaptureButton", "촬영", JelliMetaUIRuntimeBuilder.Color32(17, 24, 39), Color.white, 25f);
        JelliMetaUIRuntimeBuilder.SetFixed(captureButton.transform as RectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-44f, -56f), new Vector2(132f, 132f));
        JelliMetaUIRuntimeBuilder.EnsureRoundedBackground(captureButton.transform, "RuntimeRoundedButtonBackground", JelliMetaUIRuntimeBuilder.Color32(17, 24, 39), 66f);
        captureButton.onClick.AddListener(() => Debug.Log("[JelliMetaUIManager] 캡처 버튼 클릭"));

        TextMeshProUGUI guide = JelliMetaUIRuntimeBuilder.CreateText(
            sheet.transform,
            "Guide",
            "드래그 이동 / 우클릭 회전 / 휠 크기 조절",
            23f,
            FontStyles.Normal,
            JelliMetaUIRuntimeBuilder.Color32(107, 114, 128),
            TextAlignmentOptions.Left);
        JelliMetaUIRuntimeBuilder.SetBottom(guide.rectTransform, 40f, 210f, 24f, 44f);

        return overlay.transform;
    }

    private void DisablePanelBlockingImage(GameObject panel)
    {
        if (panel == null)
            return;

        Image panelImage = panel.GetComponent<Image>();
        if (panelImage != null)
        {
            panelImage.enabled = false;
            panelImage.raycastTarget = false;
        }
    }

    private void RemoveCameraGuide(Transform overlay)
    {
        if (overlay == null)
            return;

        Transform cameraGuide = overlay.Find("CameraGuide");
        if (cameraGuide != null)
            Destroy(cameraGuide.gameObject);
    }

    private void UpdateAROverlay(Transform overlay, ProductData rowData)
    {
        if (overlay == null)
            return;

        Image thumb = overlay.Find("BottomSheet/Thumbnail")?.GetComponent<Image>();
        if (thumb != null)
        {
            thumb.sprite = rowData.productImage;
            thumb.preserveAspect = false;
        }

        TextMeshProUGUI productName = overlay.Find("BottomSheet/ProductName")?.GetComponent<TextMeshProUGUI>();
        if (productName != null)
            productName.text = string.IsNullOrEmpty(rowData.productName) ? "선택 상품" : rowData.productName;

        TextMeshProUGUI productPrice = overlay.Find("BottomSheet/ProductPrice")?.GetComponent<TextMeshProUGUI>();
        if (productPrice != null)
            productPrice.text = string.IsNullOrEmpty(rowData.price) ? "가격 정보 없음" : rowData.price;
    }

    private void SetUILayerRecursive(GameObject target, int layer)
    {
        target.layer = layer;
        foreach (Transform child in target.transform)
            SetUILayerRecursive(child.gameObject, layer);
    }
}
