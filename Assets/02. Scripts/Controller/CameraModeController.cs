using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Orbit : 카메라의 Pivot이 모델에 고정되어 모델을 중심으로 회전함
/// FirstPerson : 1인칭 카메라 시점으로, WASD와 마우스를 이용한 자유비행모드.
/// </summary>

public class CameraModeController : Singleton<CameraModeController>
{
    #region properties
    public enum NaviMode { Orbit, FirstPerson }
    public CameraController cameraController;
    public NaviMode naviMode = NaviMode.Orbit;

    [Header("tween effect")]
    public Ease tweenEase;
    public float tweenDuration;

    [Header("mode toggle")]
    public Toggle oribitToggle;
    public Toggle firstPersonToggle;
    public Toggle showOnlyToggle;
    public Button showOnlyExit;
    #endregion

    public void SetMode(NaviMode naviMode)
    {
        this.naviMode = naviMode;
        if (naviMode == NaviMode.Orbit)
        {
            if (ModelModifier.Instance.SelectedModel)
                FocusTarget(ModelModifier.Instance.SelectedModel);
        }
        else
        {

        }
    }
    public void FocusTarget(Transform target)
    {
        if (target == null) return;

        cameraController.freezeMove = true;
        Camera.main.transform.SetParent(null);
        cameraController.cameraPivot.position = ModelModifier.RendererBound(target).center;
        Camera.main.transform.SetParent(cameraController.cameraPivot);

        Camera.main.transform.DOLocalMoveX(0f, tweenDuration).SetEase(tweenEase);
        Camera.main.transform.DOLocalMoveY(0f, tweenDuration).SetEase(tweenEase);
        Camera.main.transform.DOLocalRotate(Vector3.zero, tweenDuration).SetEase(tweenEase).OnComplete(delegate { cameraController.freezeMove = false; });
    }
    public void FocusTarget(Vector3 targetVector)
    {
        cameraController.freezeMove = true;
        Camera.main.transform.SetParent(null);
        cameraController.cameraPivot.position = targetVector;
        Camera.main.transform.SetParent(cameraController.cameraPivot);

        Camera.main.transform.DOLocalMoveX(0f, tweenDuration).SetEase(tweenEase);
        Camera.main.transform.DOLocalMoveY(0f, tweenDuration).SetEase(tweenEase);
        Camera.main.transform.DOLocalRotate(Vector3.zero, tweenDuration).SetEase(tweenEase).OnComplete(delegate { cameraController.freezeMove = false;});
    }

    #region private functions
    protected override void Awake()
    {
        base.Awake();
        oribitToggle.onValueChanged.AddListener(delegate { SetMode(NaviMode.Orbit); });
        firstPersonToggle.onValueChanged.AddListener(delegate { SetMode(NaviMode.FirstPerson); });
        showOnlyToggle.onValueChanged.AddListener(delegate { OnValueChanged_ShowOnly(showOnlyToggle); });
    }

    private void LateUpdate()
    {
        if (ModelModifier.Instance.SelectedModel == null) return;
        cameraController.ScrollZoom();
        switch (naviMode)
        {
            case NaviMode.Orbit:
                cameraController.Rotate(cameraController.cameraPivot);
                cameraController.TrackTarget(ModelModifier.Instance.SelectedModel);
                break;
            case NaviMode.FirstPerson:
                cameraController.Rotate(cameraController.cameraTransform);
                cameraController.FlyMove();
                cameraController.Panning();
                cameraController.UpDown();
                break;
            default:
                break;
        }
    }

    private void OnValueChanged_ShowOnly(Toggle toggle)
    {
        if (toggle.isOn)
        {
            if (!oribitToggle.isOn)
                oribitToggle.isOn = true;

            UILayoutManager.Instance.flipToggle.isOn = false;
            Camera.main.cullingMask = 1 << LayerMask.NameToLayer("Selected"); //해당 레이어만 렌더링
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            ViewModeController.Instance.OnClick_Quarter();
            //toggle.transform.GetChild(0).gameObject.SetActive(true);
            showOnlyExit.gameObject.SetActive(true);
        }
        else
        {
            UILayoutManager.Instance.flipToggle.isOn = true;
            Camera.main.cullingMask = CameraController.CullingMask;
            Camera.main.clearFlags = CameraClearFlags.Skybox;
        }
    }
    #endregion
}