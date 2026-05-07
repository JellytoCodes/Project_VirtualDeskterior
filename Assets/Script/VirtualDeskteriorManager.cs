using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections;

public class VirtualDeskteriorManager : MonoBehaviour
{
    public GameObject shoppingUIPanel;
    public GameObject arUIPanel;
    public Camera mainCamera;

    private ARSession arSession;
    private GameObject currentSpawnedObject;

    // --- 상호작용을 위한 상태 변수들 ---
    private bool isDragging = false;
    private float initialPinchDistance;
    private Vector3 initialScale;
    private Quaternion initialRotation;
    
    public float rotationSpeed = 0.5f; 
    public float scaleSpeed = 0.001f;
    public float minScale = 0.1f;
    public float maxScale = 5.0f;

    private void Start()
    {
        arSession = FindFirstObjectByType<ARSession>();
        
        arUIPanel.SetActive(false);
        shoppingUIPanel.SetActive(true);

        if (arSession != null)
        {
            arSession.Reset(); 
            arSession.enabled = false;
        }
    }
    void Update()
    {
        // --- 1단계: 조기 종료(Early Return) 추적 ---
        if (!arUIPanel.activeSelf) 
        {
            // 주의: 프레임마다 찍히면 로그가 폭발하므로 터치할 때만 찍어봅니다.
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                Debug.Log("[AR_DEBUG_ERROR] 실패: arUIPanel이 꺼져있다고 인식됩니다!");
            return;
        }

        if (currentSpawnedObject == null) 
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                Debug.Log("[AR_DEBUG_ERROR] 실패: 스폰된 오브젝트(currentSpawnedObject)가 null입니다!");
            return;
        }

        int touchCount = Input.touchCount;
        if (touchCount == 0)
        {
            if (isDragging)
            {
                Debug.Log("[AR_DEBUG] 드래그 완전 종료 (손가락 뗌)");
                isDragging = false;
            }
            return;
        }

        // --- 2단계: 터치 진입 확인 ---
        Touch touch = Input.GetTouch(0);
        
        if (touch.phase == TouchPhase.Began)
        {
            Debug.Log($"[AR_DEBUG] 화면 터치 감지! 터치 개수: {touchCount}, 좌표: {touch.position}");

            // ⚠️ 핵심 체크: 투명 UI 패널이 3D 터치를 막고 있는지(방패 역할) 검사
            if (UnityEngine.EventSystems.EventSystem.current != null && 
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                Debug.Log("[AR_DEBUG_WARNING] 유니티 UI(버튼이나 투명 패널)를 터치했습니다! 3D 모델링 클릭이 무시될 수 있습니다.");
                // 보통 UI를 터치하면 3D 조작을 무시하도록 return을 걸지만, 원인 파악을 위해 일단 통과시킵니다.
            }

            Ray ray = mainCamera.ScreenPointToRay(touch.position);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log($"[AR_DEBUG] 광선 적중! 맞춘 물체: {hit.collider.gameObject.name}");
                
                if (hit.collider.gameObject == currentSpawnedObject || hit.collider.transform.IsChildOf(currentSpawnedObject.transform))
                {
                    isDragging = true;
                    Debug.Log("[AR_DEBUG] 내 모델링 터치 성공! 드래그 시작");
                }
                else
                {
                    Debug.Log("[AR_DEBUG] 다른 3D 오브젝트를 맞췄습니다.");
                }
            }
            else
            {
                Debug.Log("[AR_DEBUG] 광선이 아무것도 맞추지 못했습니다 (허공 터치).");
            }
        }
        else if (touch.phase == TouchPhase.Moved && isDragging)
        {
            if (touchCount == 1)
            {
                float distanceToCamera = Vector3.Distance(mainCamera.transform.position, currentSpawnedObject.transform.position);
                Vector3 newPos = mainCamera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, distanceToCamera));
                currentSpawnedObject.transform.position = newPos;
            }
        }
        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            Debug.Log("[AR_DEBUG] 터치 종료");
            isDragging = false;
        }

        // --- 3단계: 2손가락 상호작용 (회전, 스케일) ---
        if (touchCount == 2)
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
                // 초기 회전값 저장
                initialRotation = currentSpawnedObject.transform.rotation;
                Debug.Log("[AR_DEBUG] 두 손가락 터치 시작 (회전/사이즈 조절 대기)");
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

    public void OnClickPlacementButton(GameObject modelPrefab, float spawnScale)
    {
        shoppingUIPanel.SetActive(false);
        arUIPanel.SetActive(true);

        Debug.Log("[AR_DEBUG] AR 모드 진입. 코루틴 시작");
        StartCoroutine(StartARSession(modelPrefab, spawnScale));
    }

    private IEnumerator StartARSession(GameObject modelPrefab, float spawnScale)
    {
        if (arSession != null)
        {
            arSession.enabled = true;
        }

        Debug.Log("[AR_DEBUG] AR 엔진 공간 인식 대기 중...");
        
        // 핵심 수정: AR 엔진이 현실 세계 좌표계를 완벽히 구축할 때까지 무한 대기
        while (ARSession.state != ARSessionState.SessionTracking)
        {
            yield return null;
        }

        Debug.Log("[AR_DEBUG] 공간 인식 완료! 좌표계 안정화됨. 모델링 스폰 진행");
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
            int colliderCount = 0;
            
            foreach (Renderer renderer in renderers)
            {
                if (renderer.gameObject.GetComponent<Collider>() == null)
                {
                    renderer.gameObject.AddComponent<BoxCollider>();
                    colliderCount++;
                }
            }
            Debug.Log($"[AR_DEBUG] 스폰 완료. 총 {colliderCount}개의 BoxCollider가 자동 부착되었습니다.");
        }
    }

    public void ReturnToShoppingUI()
    {
        arUIPanel.SetActive(false);
        shoppingUIPanel.SetActive(true);

        Debug.Log("[AR_DEBUG] 쇼핑몰 UI로 복귀. AR 세션 리셋");

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