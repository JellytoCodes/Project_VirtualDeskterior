using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;

public class ARInteractionManager : MonoBehaviour
{
    [Header("Core")]
    public Camera mainCamera;
    public bool editorPreviewMode = true;

    [Header("Object Interaction")]
    public float rotationSpeed = 0.5f;
    public float minScale = 0.1f;
    public float maxScale = 5.0f;

    private ARSession arSession;
    private GameObject currentSpawnedObject;

    private bool isDragging = false;
    private float currentDragDepth;
    private float initialPinchDistance;
    private Vector3 initialScale;

    private void Awake()
    {
        ResolveCamera();
        arSession = FindAnyObjectByType<ARSession>();
    }

    private void Start()
    {
        StopARSession();
    }

    private void Update()
    {
        if (currentSpawnedObject == null)
            return;

        ResolveCamera();
        if (mainCamera == null)
            return;

#if UNITY_EDITOR
        if (editorPreviewMode)
        {
            HandleEditorMouseInput();
            return;
        }
#endif

        if (arSession == null || !arSession.enabled)
            return;

        HandleTouchInput();
    }

    public void StartARPlacement(GameObject modelPrefab, float spawnScale)
    {
        StopCoroutine(nameof(ARSessionRoutine));
        StartCoroutine(ARSessionRoutine(modelPrefab, spawnScale));
    }

    private IEnumerator ARSessionRoutine(GameObject modelPrefab, float spawnScale)
    {
#if UNITY_EDITOR
        if (editorPreviewMode)
        {
            SpawnModel(modelPrefab, spawnScale);
            Debug.Log("[ARInteractionManager] Editor Preview Mode: 모델을 카메라 앞에 생성했습니다.");
            yield break;
        }
#endif

        if (arSession != null)
        {
            arSession.enabled = true;

            while (ARSession.state != ARSessionState.SessionTracking)
                yield return null;
        }

        SpawnModel(modelPrefab, spawnScale);
    }

    private void SpawnModel(GameObject modelPrefab, float spawnScale)
    {
        ResolveCamera();

        if (mainCamera == null)
        {
            Debug.LogError("[ARInteractionManager] Main Camera를 찾지 못했습니다.");
            return;
        }

        if (currentSpawnedObject != null)
        {
            Destroy(currentSpawnedObject);
            currentSpawnedObject = null;
        }

        Vector3 spawnPos = mainCamera.transform.position + mainCamera.transform.forward * 1.5f;
        spawnPos.y -= 0.25f;

        Quaternion spawnRot = Quaternion.Euler(0f, mainCamera.transform.eulerAngles.y + 180f, 0f);

        if (modelPrefab != null)
        {
            currentSpawnedObject = Instantiate(modelPrefab, spawnPos, spawnRot);
        }
        else
        {
            currentSpawnedObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            currentSpawnedObject.name = "AR Preview Placeholder Cube";
            currentSpawnedObject.transform.SetPositionAndRotation(spawnPos, spawnRot);
            Debug.LogWarning("[ARInteractionManager] 선택 상품의 arModelPrefab이 비어 있어 테스트용 큐브를 생성했습니다.");
        }

        float safeScale = spawnScale > 0f ? spawnScale : 0.3f;
        currentSpawnedObject.transform.localScale = Vector3.one * safeScale;
        currentSpawnedObject.SetActive(true);

        AddMissingColliders(currentSpawnedObject);
        FocusCameraForEditorPreview();

        Debug.Log($"[ARInteractionManager] 생성 완료: {currentSpawnedObject.name} / position={currentSpawnedObject.transform.position} / scale={currentSpawnedObject.transform.localScale}");
    }

    private void ResolveCamera()
    {
        if (mainCamera != null)
            return;

        mainCamera = Camera.main;

        if (mainCamera == null)
            mainCamera = FindAnyObjectByType<Camera>();
    }

    private void FocusCameraForEditorPreview()
    {
#if UNITY_EDITOR
        if (!editorPreviewMode || currentSpawnedObject == null || mainCamera == null)
            return;

        mainCamera.transform.LookAt(currentSpawnedObject.transform.position + Vector3.up * 0.1f);
#endif
    }

    private void AddMissingColliders(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = true;

            if (renderer.GetComponent<Collider>() == null)
                renderer.gameObject.AddComponent<BoxCollider>();
        }

        if (renderers.Length == 0 && root.GetComponent<Collider>() == null)
            root.AddComponent<BoxCollider>();
    }

    private void HandleTouchInput()
    {
        int touchCount = Input.touchCount;

        if (touchCount == 0)
        {
            isDragging = false;
            return;
        }

        Touch touch = Input.GetTouch(0);

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            return;

        if (touchCount == 1)
        {
            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = mainCamera.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider.gameObject == currentSpawnedObject || hit.collider.transform.IsChildOf(currentSpawnedObject.transform))
                    {
                        isDragging = true;
                        currentDragDepth = mainCamera.WorldToScreenPoint(currentSpawnedObject.transform.position).z;
                    }
                }
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                Vector3 newPos = mainCamera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, currentDragDepth));
                currentSpawnedObject.transform.position = newPos;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }
        else if (touchCount == 2)
        {
            isDragging = false;

            Touch touch1 = Input.GetTouch(1);
            Vector2 curTouch0Pos = touch.position;
            Vector2 curTouch1Pos = touch1.position;
            Vector2 prevTouch0Pos = curTouch0Pos - touch.deltaPosition;
            Vector2 prevTouch1Pos = curTouch1Pos - touch1.deltaPosition;

            if (touch1.phase == TouchPhase.Began)
            {
                initialPinchDistance = Vector2.Distance(prevTouch0Pos, prevTouch1Pos);
                initialScale = currentSpawnedObject.transform.localScale;
            }
            else if (touch.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
            {
                Vector2 prevDir = prevTouch0Pos - prevTouch1Pos;
                Vector2 curDir = curTouch0Pos - curTouch1Pos;

                if (prevDir != Vector2.zero && curDir != Vector2.zero)
                {
                    float angle = Vector2.SignedAngle(prevDir, curDir);
                    currentSpawnedObject.transform.Rotate(Vector3.up, angle * rotationSpeed, Space.World);
                }

                float curPinchDistance = Vector2.Distance(curTouch0Pos, curTouch1Pos);
                if (initialPinchDistance > 0)
                {
                    float scaleFactor = curPinchDistance / initialPinchDistance;
                    SetObjectScale(initialScale * scaleFactor);
                }
            }
        }
    }

#if UNITY_EDITOR
    private void HandleEditorMouseInput()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            Vector3 scaleDelta = currentSpawnedObject.transform.localScale + Vector3.one * scroll * 0.03f;
            SetObjectScale(scaleDelta);
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject == currentSpawnedObject || hit.collider.transform.IsChildOf(currentSpawnedObject.transform))
                {
                    isDragging = true;
                    currentDragDepth = mainCamera.WorldToScreenPoint(currentSpawnedObject.transform.position).z;
                }
            }
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 newPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, currentDragDepth));
            currentSpawnedObject.transform.position = newPos;
        }

        if (Input.GetMouseButtonUp(0))
            isDragging = false;

        if (Input.GetMouseButton(1))
        {
            float deltaX = Input.GetAxis("Mouse X");
            currentSpawnedObject.transform.Rotate(Vector3.up, -deltaX * 3.0f, Space.World);
        }
    }
#endif

    private void SetObjectScale(Vector3 targetScale)
    {
        targetScale.x = Mathf.Clamp(targetScale.x, minScale, maxScale);
        targetScale.y = Mathf.Clamp(targetScale.y, minScale, maxScale);
        targetScale.z = Mathf.Clamp(targetScale.z, minScale, maxScale);
        currentSpawnedObject.transform.localScale = targetScale;
    }

    public void StopARSession()
    {
        if (arSession != null)
        {
            arSession.Reset();
            arSession.enabled = false;
        }

        if (currentSpawnedObject != null)
        {
            Destroy(currentSpawnedObject);
            currentSpawnedObject = null;
        }

        isDragging = false;
    }
}
