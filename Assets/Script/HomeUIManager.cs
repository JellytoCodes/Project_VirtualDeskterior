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
    public List<string> categoryList = new List<string>() { "전체", "책상", "의자", "모니터", "조명", "소품", "수납" };

    [Header("3. Product Grid Setup")]
    public Transform productGridParent;
    public GameObject productCardPrefab;
    public ProductDataSO masterDatabase;

    private string selectedCategory = "전체";

    private void OnEnable()
    {
        if (mainUIManager == null)
            mainUIManager = FindAnyObjectByType<JelliMetaUIManager>();

        NormalizeHomeLayout();
        SetupHeroBanner();
        GenerateCategoryMenu();
        GenerateProductGrid();
    }

    private void SetupHeroBanner()
    {
        if (bannerThumbnailImage != null)
        {
            if (currentBannerData != null && currentBannerData.bannerImage != null)
                bannerThumbnailImage.sprite = currentBannerData.bannerImage;
            bannerThumbnailImage.preserveAspect = true;
            bannerThumbnailImage.color = Color.white;
        }

        string title = currentBannerData != null && !string.IsNullOrEmpty(currentBannerData.bannerTitle)
            ? currentBannerData.bannerTitle
            : "미니멀 데스크테리어";

        if (bannerTitleText != null)
            JelliMetaUIRuntimeBuilder.ConfigureText(bannerTitleText, title, 44f, FontStyles.Bold, JelliMetaUIRuntimeBuilder.Color32(17, 24, 39), TextAlignmentOptions.Left);

        if (bannerDetailButton != null)
        {
            bannerDetailButton.onClick.RemoveAllListeners();
            bannerDetailButton.onClick.AddListener(() => Debug.Log("[HomeUIManager] 배너 클릭됨"));

            Image buttonBg = bannerDetailButton.GetComponent<Image>();
            if (buttonBg != null)
                buttonBg.color = JelliMetaUIRuntimeBuilder.Color32(17, 24, 39);

            TextMeshProUGUI label = bannerDetailButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
                JelliMetaUIRuntimeBuilder.ConfigureText(label, "자세히 보기", 24f, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);
        }
    }

    private void GenerateCategoryMenu()
    {
        if (categoryContentParent == null || categoryPillPrefab == null)
            return;

        foreach (Transform child in categoryContentParent)
            Destroy(child.gameObject);

        foreach (string categoryName in categoryList)
        {
            GameObject newCategoryObj = Instantiate(categoryPillPrefab, categoryContentParent);
            CategoryButtonUI categoryUI = newCategoryObj.GetComponent<CategoryButtonUI>();
            if (categoryUI != null)
                categoryUI.SetupCategory(categoryName, OnCategorySelected, categoryName == selectedCategory);
        }

        NormalizeCategoryContent();
    }

    private void OnCategorySelected(string categoryName)
    {
        selectedCategory = string.IsNullOrEmpty(categoryName) ? "전체" : categoryName;
        GenerateCategoryMenu();
        GenerateProductGrid();
    }

    public void GenerateProductGrid()
    {
        if (productGridParent == null || productCardPrefab == null)
        {
            Debug.LogError("[HomeUIManager] 상품 그리드 Parent 또는 ProductCard Prefab이 연결되지 않았습니다.");
            return;
        }

        foreach (Transform child in productGridParent)
            Destroy(child.gameObject);

        if (masterDatabase == null)
        {
            Debug.LogError("[HomeUIManager] 마스터 DB가 연결되지 않았습니다.");
            return;
        }

        int visibleCount = 0;
        foreach (ProductData rowData in masterDatabase.productList)
        {
            if (!IsVisibleInSelectedCategory(rowData))
                continue;

            GameObject newCard = Instantiate(productCardPrefab, productGridParent);
            ProductCardUI cardUI = newCard.GetComponent<ProductCardUI>();

            if (cardUI != null)
                cardUI.SetupCard(rowData, mainUIManager);

            visibleCount++;
        }

        NormalizeProductGrid(visibleCount);
        JelliMetaUIRuntimeBuilder.ApplyProjectFonts(gameObject);
    }

    private bool IsVisibleInSelectedCategory(ProductData rowData)
    {
        if (string.IsNullOrEmpty(selectedCategory) || selectedCategory == "전체")
            return true;

        if (!string.IsNullOrEmpty(rowData.category) && rowData.category == selectedCategory)
            return true;

        string haystack = $"{rowData.productName} {rowData.description}";
        return haystack.Contains(selectedCategory);
    }

    private void NormalizeHomeLayout()
    {
        JelliMetaUIRuntimeBuilder.ConfigureCanvasForMobile(gameObject);

        RectTransform homeRect = transform as RectTransform;
        JelliMetaUIRuntimeBuilder.SetStretch(homeRect, 0f, 0f, 0f, 0f);
        ForceWhiteBackground(gameObject);

        Transform header = transform.Find("Header");
        if (header != null)
        {
            // Header는 Figma 기준으로 앱명 + 검색바가 한 블록 안에 들어가야 한다.
            // 기존 Header의 VerticalLayoutGroup이 자식 좌표를 다시 밀어버리고 있었으므로 수동 배치로 전환한다.
            JelliMetaUIRuntimeBuilder.SetTop(header as RectTransform, 0f, 0f, 0f, 178f);
            ForceWhiteBackground(header.gameObject);

            VerticalLayoutGroup headerLayout = header.GetComponent<VerticalLayoutGroup>();
            if (headerLayout != null)
                headerLayout.enabled = false;

            ContentSizeFitter headerFitter = header.GetComponent<ContentSizeFitter>();
            if (headerFitter != null)
                headerFitter.enabled = false;
        }

        TextMeshProUGUI logo = GetOrCreateHeaderLogo(header);
        if (logo != null)
        {
            LayoutElement logoLayout = JelliMetaUIRuntimeBuilder.EnsureComponent<LayoutElement>(logo.gameObject);
            logoLayout.ignoreLayout = true;

            JelliMetaUIRuntimeBuilder.SetFixed(
                logo.rectTransform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(48f, -22f),
                new Vector2(520f, 54f)
            );

            JelliMetaUIRuntimeBuilder.ConfigureText(logo, "JelliMeta", 40f, FontStyles.Normal, Color.black, TextAlignmentOptions.Left);
            logo.gameObject.SetActive(true);
            logo.transform.SetAsLastSibling();
        }

        Transform search = header != null ? header.Find("SearchBar") : transform.Find("Header/SearchBar");
        if (search != null)
        {
            LayoutElement searchLayout = JelliMetaUIRuntimeBuilder.EnsureComponent<LayoutElement>(search.gameObject);
            searchLayout.ignoreLayout = true;

            // 기존보다 위로 올려 로고와 같은 Header 블록 안에 정렬한다.
            JelliMetaUIRuntimeBuilder.SetTop(search as RectTransform, 48f, 48f, 82f, 64f);
            Image searchBg = JelliMetaUIRuntimeBuilder.EnsureComponent<Image>(search.gameObject);
            searchBg.color = JelliMetaUIRuntimeBuilder.Color32(249, 250, 251);
            searchBg.raycastTarget = true;

            TextMeshProUGUI placeholder = search.Find("Text Area/Placeholder")?.GetComponent<TextMeshProUGUI>();
            if (placeholder != null)
                JelliMetaUIRuntimeBuilder.ConfigureText(placeholder, "가구, 소품 검색", 27f, FontStyles.Normal, JelliMetaUIRuntimeBuilder.Color32(107, 114, 128), TextAlignmentOptions.Left);

            search.SetSiblingIndex(0);
        }

        // SearchBar를 먼저 그리고, Text_Logo를 마지막에 그려 Z Order를 확실히 위로 둔다.
        if (logo != null)
            logo.transform.SetAsLastSibling();

        Transform mainScroll = transform.Find("MainScrollArea");
        if (mainScroll != null)
        {
            JelliMetaUIRuntimeBuilder.SetStretch(mainScroll as RectTransform, 0f, 0f, 178f, 120f);
            ForceWhiteBackground(mainScroll.gameObject);
        }

        Transform viewport = transform.Find("MainScrollArea/Viewport");
        if (viewport != null)
            ForceWhiteBackground(viewport.gameObject);

        Transform content = transform.Find("MainScrollArea/Viewport/Content");
        if (content != null)
        {
            ForceWhiteBackground(content.gameObject);

            RectTransform contentRect = content as RectTransform;
            if (contentRect != null)
            {
                contentRect.anchorMin = new Vector2(0f, 1f);
                contentRect.anchorMax = new Vector2(1f, 1f);
                contentRect.pivot = new Vector2(0.5f, 1f);
                contentRect.offsetMin = new Vector2(0f, contentRect.offsetMin.y);
                contentRect.offsetMax = new Vector2(0f, contentRect.offsetMax.y);
            }

            VerticalLayoutGroup layout = JelliMetaUIRuntimeBuilder.EnsureComponent<VerticalLayoutGroup>(content.gameObject);
            layout.padding = new RectOffset(0, 0, 28, 44);
            layout.spacing = 24f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = JelliMetaUIRuntimeBuilder.EnsureComponent<ContentSizeFitter>(content.gameObject);
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        NormalizeHeroBannerLayout();
        NormalizeCategoryMenuLayout();
        NormalizeProductGrid(masterDatabase != null ? masterDatabase.productList.Count : 0);
        JelliMetaUIRuntimeBuilder.ApplyProjectFonts(gameObject);
    }

    private TextMeshProUGUI GetOrCreateHeaderLogo(Transform header)
    {
        if (header == null)
            return null;

        Transform existing = header.Find("Text_Logo");
        TextMeshProUGUI logo = existing != null ? existing.GetComponent<TextMeshProUGUI>() : null;
        if (logo != null)
            return logo;

        return JelliMetaUIRuntimeBuilder.CreateText(header, "Text_Logo", "JelliMeta", 40f, FontStyles.Normal, Color.black, TextAlignmentOptions.Left);
    }

    private void ForceWhiteBackground(GameObject target)
    {
        if (target == null)
            return;

        Image image = JelliMetaUIRuntimeBuilder.EnsureComponent<Image>(target);
        if (image == null)
            return;

        image.color = Color.white;
        image.raycastTarget = false;
    }

    private void NormalizeHeroBannerLayout()
    {
        Transform hero = transform.Find("MainScrollArea/Viewport/Content/HeroBennerArea");
        if (hero == null)
            return;

        LayoutElement heroLayout = JelliMetaUIRuntimeBuilder.EnsureComponent<LayoutElement>(hero.gameObject);
        heroLayout.minHeight = 360f;
        heroLayout.preferredHeight = 360f;
        heroLayout.flexibleHeight = 0f;

        Transform bannerBg = hero.Find("BannerBG");
        if (bannerBg != null)
        {
            JelliMetaUIRuntimeBuilder.SetStretch(bannerBg as RectTransform, 48f, 48f, 0f, 0f);
            Image bg = JelliMetaUIRuntimeBuilder.EnsureComponent<Image>(bannerBg.gameObject);
            bg.color = JelliMetaUIRuntimeBuilder.Color32(239, 246, 255);
        }

        if (bannerTitleText != null)
            JelliMetaUIRuntimeBuilder.SetFixed(bannerTitleText.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(88f, 52f), new Vector2(500f, 118f));

        if (bannerDetailButton != null)
            JelliMetaUIRuntimeBuilder.SetFixed(bannerDetailButton.transform as RectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(88f, -86f), new Vector2(230f, 62f));

        if (bannerThumbnailImage != null)
            JelliMetaUIRuntimeBuilder.SetFixed(bannerThumbnailImage.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-88f, 0f), new Vector2(260f, 260f));
    }

    private void NormalizeCategoryMenuLayout()
    {
        Transform categoryMenu = transform.Find("MainScrollArea/Viewport/Content/CategoryMenu");
        if (categoryMenu == null)
            return;

        LayoutElement layout = JelliMetaUIRuntimeBuilder.EnsureComponent<LayoutElement>(categoryMenu.gameObject);
        layout.minHeight = 92f;
        layout.preferredHeight = 92f;
        layout.flexibleHeight = 0f;

        ScrollRect scroll = categoryMenu.GetComponent<ScrollRect>();
        if (scroll != null)
        {
            scroll.horizontal = true;
            scroll.vertical = false;
        }

        NormalizeCategoryContent();
    }

    private void NormalizeCategoryContent()
    {
        if (categoryContentParent == null)
            return;

        RectTransform contentRect = categoryContentParent as RectTransform;
        if (contentRect != null)
        {
            contentRect.anchorMin = new Vector2(0f, 0.5f);
            contentRect.anchorMax = new Vector2(0f, 0.5f);
            contentRect.pivot = new Vector2(0f, 0.5f);
            contentRect.anchoredPosition = new Vector2(48f, 0f);
            contentRect.localScale = Vector3.one;
        }

        HorizontalLayoutGroup group = JelliMetaUIRuntimeBuilder.EnsureComponent<HorizontalLayoutGroup>(categoryContentParent.gameObject);
        group.padding = new RectOffset(0, 48, 10, 10);
        group.spacing = 16f;
        group.childAlignment = TextAnchor.MiddleLeft;
        group.childControlWidth = true;
        group.childControlHeight = true;
        group.childForceExpandWidth = false;
        group.childForceExpandHeight = false;

        ContentSizeFitter fitter = JelliMetaUIRuntimeBuilder.EnsureComponent<ContentSizeFitter>(categoryContentParent.gameObject);
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
    }

    private void NormalizeProductGrid(int visibleCount)
    {
        if (productGridParent == null)
            return;

        GridLayoutGroup grid = JelliMetaUIRuntimeBuilder.EnsureComponent<GridLayoutGroup>(productGridParent.gameObject);
        grid.padding = new RectOffset(48, 48, 0, 0);
        grid.spacing = new Vector2(32f, 42f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;
        grid.childAlignment = TextAnchor.UpperLeft;

        float canvasWidth = 1080f;
        RectTransform parentRect = productGridParent.parent as RectTransform;
        if (parentRect != null && parentRect.rect.width > 1f)
            canvasWidth = parentRect.rect.width;

        float cellWidth = (canvasWidth - grid.padding.left - grid.padding.right - grid.spacing.x) / 2f;
        float cellHeight = cellWidth + 124f;
        grid.cellSize = new Vector2(cellWidth, cellHeight);

        int rowCount = Mathf.Max(1, Mathf.CeilToInt(visibleCount / 2f));
        float preferredHeight = grid.padding.top + grid.padding.bottom + rowCount * cellHeight + Mathf.Max(0, rowCount - 1) * grid.spacing.y;

        LayoutElement gridLayout = JelliMetaUIRuntimeBuilder.EnsureComponent<LayoutElement>(productGridParent.gameObject);
        gridLayout.minHeight = preferredHeight;
        gridLayout.preferredHeight = preferredHeight;
        gridLayout.flexibleHeight = 0f;

        RectTransform rect = productGridParent as RectTransform;
        if (rect != null)
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, preferredHeight);
    }
}
