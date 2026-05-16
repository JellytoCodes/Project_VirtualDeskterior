using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections;
using UnityEngine.EventSystems;

public class ARInteractionManager : MonoBehaviour
{
    public Camera mainCamera;
    private ARSession arSession;
    private GameObject currentSpawnedObject;

    // --- 상호작용 상태 변수 ---
    private bool isDragging = false;
    private float currentDragDepth; 
    private float initialPinchDistance;
    private Vector3 initialScale;
    
    public float rotationSpeed = 0.5f; 
    public float minScale = 0.1f;
    public float maxScale = 5.0f;

    private void Start()
    {
        arSession = FindFirstObjectByType<ARSession>();
        StopARSession();
    }

    void Update()
    {
        // 스폰된 오브젝트가 없거나 AR 세션이 꺼져있으면 작동안함
        if (currentSpawnedObject == null || arSession == null || !arSession.enabled) return;

        int touchCount = Input.touchCount;
        
        if (touchCount == 0)
        {
            isDragging = false;
            return;
        }

        Touch touch = Input.GetTouch(0);

        // 유니티 UI(투명 패널, 버튼 등) 터치 시 3D 조작 무시
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
        {
            return;
        }

        // --- 1. 한 손가락 터치: 이동 ---
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
        // --- 2. 두 손가락 터치: 회전 및 사이즈 조절 ---
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
                // 회전 적용
                Vector2 prevDir = prevTouch0Pos - prevTouch1Pos;
                Vector2 curDir = curTouch0Pos - curTouch1Pos;
                if (prevDir != Vector2.zero && curDir != Vector2.zero)
                {
                    float angle = Vector2.SignedAngle(prevDir, curDir);
                    currentSpawnedObject.transform.Rotate(Vector3.up, angle * rotationSpeed, Space.World);
                }

                // 스케일 적용
                float curPinchDistance = Vector2.Distance(curTouch0Pos, curTouch1Pos);
                if (initialPinchDistance > 0)
                {
                    float scaleFactor = curPinchDistance / initialPinchDistance;
                    Vector3 newScale = initialScale * scaleFactor;
                    
                    newScale.x = Mathf.Clamp(newScale.x, minScale, maxScale);
                    newScale.y = Mathf.Clamp(newScale.y, minScale, maxScale);
                    newScale.z = Mathf.Clamp(newScale.z, minScale, maxScale);
                    
                    currentSpawnedObject.transform.localScale = newScale;
                }
            }
        }
    }

    public void StartARPlacement(GameObject modelPrefab, float spawnScale)
    {
        StartCoroutine(ARSessionRoutine(modelPrefab, spawnScale));
    }

    private IEnumerator ARSessionRoutine(GameObject modelPrefab, float spawnScale)
    {
        if (arSession != null) arSession.enabled = true;

        while (ARSession.state != ARSessionState.SessionTracking)
        {
            yield return null;
        }

        SpawnModel(modelPrefab, spawnScale);
    }

    private void SpawnModel(GameObject modelPrefab, float spawnScale)
    {
        if (currentSpawnedObject == null)
        {
            Vector3 spawnPos = mainCamera.transform.position + mainCamera.transform.forward * 1.0f;
            spawnPos.y -= 0.2f;

            Quaternion spawnRot = Quaternion.LookRotation(mainCamera.transform.position - spawnPos);
            spawnRot.x = 0; spawnRot.z = 0;

            currentSpawnedObject = Instantiate(modelPrefab, spawnPos, spawnRot);
            currentSpawnedObject.transform.localScale = Vector3.one * spawnScale;

            Renderer[] renderers = currentSpawnedObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (renderer.gameObject.GetComponent<Collider>() == null)
                {
                    renderer.gameObject.AddComponent<BoxCollider>();
                }
            }
        }
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
    }
}