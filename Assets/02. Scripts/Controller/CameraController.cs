using UnityEngine;
using DG.Tweening;

/// <summary>
/// 카메라 동작 정의 스크립트
/// 구현은 CameraModeCotroller에서.
/// </summary>
public class CameraController : MonoBehaviour
{
    #region Properties
    public static LayerMask CullingMask;
    public Transform cameraTransform;
    public Transform cameraPivot;
    [Header("Camera Speed")]
    public float moveSpeed;
    public float panningSpeed;
    public float rotationSpeed;
    public float zoomSpeed;
    public float correctionFactor = 0.1f; //보정 계수
    public float smoothFactor;

    [Header("Constraint")]
    public bool freezeMove;
    public bool freezeRotation;
    public float minDistance;
    public float maxDistance;

    float correctedSpeed;
    float newDist;
    bool isRotating;

    Vector3 originalPosition;
    Vector3 originalRotation;
    Vector3 modelPivot_originalPosition;
    Vector3 modelPivot_originalRotation;
    Quaternion newRotation;
    #endregion

    #region EventHandler
    public void OnInitialized(object sender, LoadDataEventArgs e)
    {
        Bounds bounds = ModelModifier.RendererBound(e.item.transform);
        cameraPivot.transform.position = bounds.center;
    }
    private void OnReleased(object sender, Transform targetModel)
    {
        cameraTransform.GetComponent<Camera>().cullingMask = CullingMask;
    }

    private void Start()
    {
        if (ProcessManager.Instance)
        {
            ProcessManager.Instance.OnLoadedHandler += OnInitialized;
            ProcessManager.Instance.OnReleasedHandler += OnReleased;
        }
    }

    private void OnDestroy()
    {
        if (ProcessManager.Instance)
        {
            ProcessManager.Instance.OnLoadedHandler -= OnInitialized;
            ProcessManager.Instance.OnReleasedHandler -= OnReleased;
        }
    }
    #endregion

    private void Awake()
    {
        CullingMask = GetComponent<Camera>().cullingMask;

        originalPosition = cameraTransform.localPosition;
        originalRotation = cameraTransform.localEulerAngles;

        modelPivot_originalPosition = cameraPivot.localPosition;
        modelPivot_originalRotation = cameraPivot.localEulerAngles;
    }
    /// <summary>
    ///카메라의 pivot이 target의 renderer center를 항상 따라다니도록 함
    /// </summary>
    /// <param name="target"></param>
    public void TrackTarget(Transform target)
    {
        if (target != null && !freezeMove)
        {
            Vector3 followPos = ModelModifier.RendererBound(target).center;
            cameraPivot.position = Vector3.Lerp(cameraPivot.position, followPos, 0.15f);
        }
    }

    /// <summary>
    /// 카메라 회전. pivot은 회전 주체
    /// </summary>
    /// <param name="pivot"></param>
    public void Rotate(Transform pivot)
    {
        if (DOTween.IsTweening(cameraTransform) || (!isRotating && MouseEventManager.IsPointerOverUIElement()))
        {
            newRotation = pivot.rotation;
            return;
        }
        if (!freezeRotation)
        {
            if (!MouseEventManager.IsClickDelayTimeout())
            {
                if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
                {
                    isRotating = true;

                    newDist = (cameraTransform.position - cameraPivot.position).magnitude;
                    correctedSpeed = rotationSpeed + (newDist * correctionFactor);

                    float rotationX = -Input.GetAxis("Mouse Y") * rotationSpeed;
                    float rotationY = Input.GetAxis("Mouse X") * rotationSpeed;

                    Vector3 currentEulerAngles = pivot.eulerAngles;
                    currentEulerAngles.z = 0f;
                    float newRotationX = currentEulerAngles.x + rotationX;
                    float newRotationY = currentEulerAngles.y + rotationY;

                    if (newRotationX > 180f) newRotationX -= 360f;
                    newRotationX = Mathf.Clamp(newRotationX, -80f, 80f);

                    newRotation = Quaternion.Euler(newRotationX, newRotationY, 0f);
                }
                else isRotating = false;
            }
            else isRotating = false;
        }
        pivot.rotation = Quaternion.Slerp(pivot.rotation, newRotation, Time.deltaTime / smoothFactor);
    }
    /// <summary>
    /// 마우스 휠로 카메라를 x, y축으로 이동함
    /// </summary>
    public void Panning()
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        if (!freezeMove && !isRotating && Input.GetMouseButton(2))
        {
            newDist = (cameraTransform.position - cameraPivot.position).magnitude;
            correctedSpeed = panningSpeed + (newDist * correctionFactor);
            Vector3 moveDelta = -(cameraTransform.right * Input.GetAxis("Mouse X") + cameraTransform.up * Input.GetAxis("Mouse Y"))
                               * panningSpeed * Time.smoothDeltaTime;

            cameraTransform.position = Vector3.Lerp(cameraTransform.position, cameraTransform.position + moveDelta, 0.2f);
        }
#elif UNITY_ANDROID
        if (!freezeMove && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                Vector3 moveDelta = -(cameraTransform.right * touch.deltaPosition.x + cameraTransform.up * touch.deltaPosition.y)
                                    * 0.02f * panningSpeed * Time.smoothDeltaTime;

                cameraTransform.position = Vector3.Lerp(cameraTransform.position, cameraTransform.position + moveDelta, 0.2f);
            }
        }
#endif
    }
    /// <summary>
    /// 마우스 휠로 카메라 줌/인아웃
    /// </summary>
    public void ScrollZoom()
    {
        if (isRotating || MouseEventManager.IsPointerOverUIElement()) return;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        if (Input.GetAxis("Mouse ScrollWheel") != 0.0f)
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0 && ((cameraTransform.position - cameraPivot.position).magnitude < minDistance))
            {
                return;
            }
            newDist = (cameraTransform.position - cameraPivot.position).magnitude;
            correctedSpeed = zoomSpeed + (newDist * correctionFactor);
            Vector3 power = Mathf.Sign(Input.GetAxis("Mouse ScrollWheel")) * cameraTransform.forward * correctedSpeed * Time.smoothDeltaTime;

            if (CameraModeController.Instance.naviMode == CameraModeController.NaviMode.Orbit)
            {
                if (Input.GetAxis("Mouse ScrollWheel") > 0 && ((cameraTransform.position + power) - cameraPivot.position).magnitude < minDistance)
                {
                    return;
                }
            }

            //cameraTransform.position = Vector3.Lerp(cameraTransform.position, cameraTransform.position + power, 1f);
            cameraTransform.DOMove(cameraTransform.position + power, 0.4f);
            
        }
#elif UNITY_ANDROID
        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            if (touch1.phase == TouchPhase.Began && touch2.phase == TouchPhase.Began)
            {
                lastDist = Vector2.Distance(touch1.position, touch2.position);
            }

            if (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved)
            {
                float newDist = Vector2.Distance(touch1.position, touch2.position);
                touchDist = lastDist - newDist;
                lastDist = newDist;

                //Camera.main.fieldOfView += touchDist * 0.1f;

                Vector3 power = touchDist * cameraTransform.forward * 0.02f * Time.smoothDeltaTime;
                cameraTransform.DOMove(cameraTransform.position - power, 0.2f);
            }
        }
#endif
    }
    /// <summary>
    /// WASD, 마우스로 자유비행 이동
    /// </summary>
    public void FlyMove()
    {
        if (!freezeMove)
        {
            if (!MouseEventManager.IsClickDelayTimeout(0.1f))
            {
                if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
                    if (Input.GetAxis("Horizontal") != 0f || Input.GetAxis("Vertical") != 0f)
                    {
                        newDist = (cameraTransform.position - cameraPivot.position).magnitude;
                        correctedSpeed = moveSpeed + (newDist * correctionFactor);
                        Vector3 power = (Input.GetAxis("Horizontal") * cameraTransform.right + Input.GetAxis("Vertical") * cameraTransform.forward)
                                        * correctedSpeed * Time.smoothDeltaTime;

                        cameraTransform.position = Vector3.Lerp(cameraTransform.position, cameraTransform.position + power, 0.2f);
                    }
            }
        }
    }
    /// <summary>
    /// Q,E버튼으로 y축 위아래 이동
    /// </summary>
    public void UpDown()
    {
        if (!freezeMove)
        {
            if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.PageUp))
            {
                newDist = (cameraTransform.position - cameraPivot.position).magnitude;
                correctedSpeed = moveSpeed + (newDist * correctionFactor);
                Vector3 power = cameraTransform.position + Vector3.up * 0.2f * correctedSpeed * Time.smoothDeltaTime;
                cameraTransform.position = Vector3.Lerp(cameraTransform.position, power, 0.2f);
            }
            if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.PageDown))
            {
                newDist = (cameraTransform.position - cameraPivot.position).magnitude;
                correctedSpeed = moveSpeed + (newDist * correctionFactor);
                Vector3 power = cameraTransform.position + Vector3.down * 0.2f *  correctedSpeed * Time.smoothDeltaTime;
                cameraTransform.position = Vector3.Lerp(cameraTransform.position, power, 0.2f);
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tweenEffect"></param>
    /// <param name="OnComplete"></param>
    /// 

    public void InitializeState(bool tweenEffect = false, Function OnComplete = null)
    {
        isRotating = false;

        if (tweenEffect)
        {
            cameraPivot.DOKill();
            cameraPivot.DOLocalRotate(modelPivot_originalRotation, 0.2f)
                .OnComplete(delegate { cameraPivot.DOLocalMove(modelPivot_originalPosition, 0.2f); })
                .OnComplete(delegate { OnComplete?.Invoke(); })
                .OnComplete(delegate
                {
                    cameraTransform.DOKill();
                    cameraTransform.DOLocalRotate(originalRotation, 0.2f)
                        .OnComplete(delegate { cameraTransform.DOLocalMove(originalPosition, 0.2f); })
                        .OnComplete(delegate { OnComplete?.Invoke(); });
                });
        }
        else
        {
            cameraPivot.localPosition = modelPivot_originalPosition;
            cameraPivot.localEulerAngles = modelPivot_originalRotation;

            cameraTransform.localPosition = originalPosition;
            cameraTransform.localEulerAngles = originalRotation;
        }
    }

}