using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class MinimapController : MonoBehaviour
{
    #region properties
    public Texture minimapGraphicTarget;
    public Camera minimapCam;
    public Transform Minimap;
    public Transform playerPoint;
    public Transform playerRotateMarker;

    [Header("Options")]
    public Slider levelSlider;
    public Toggle minimapActiveToggle;
    public Toggle projectionToggle;

    [Header("MinMaxValue")]
    public float OrthographMinValue;
    public float OrthographMaxValue;

    [Header("MinMaxValue")]
    public float PerspectiveMinValue;
    public float PerspectiveMaxValue;
    #endregion

    #region private functions
    private void Awake()
    {
        Minimap.GetComponentInChildren<RawImage>(true).texture = minimapGraphicTarget;
        minimapActiveToggle.onValueChanged.AddListener(delegate { OnValueChanged_ActiveToggle(minimapActiveToggle); });
        projectionToggle.onValueChanged.AddListener(delegate { OnValueChanged_ProjectionToggle(projectionToggle); });
        levelSlider.onValueChanged.AddListener(delegate { OnValueChanged_MapToggle(levelSlider); });
        minimapActiveToggle.isOn = false;

        OnValueChanged_ProjectionToggle(projectionToggle);
    }
    private void Start()
    {
        if (ProcessManager.Instance)
            ProcessManager.Instance.OnLoadedHandler += OnInitialized;
    }
    private void OnDestroy()
    {
        if (ProcessManager.Instance)
            ProcessManager.Instance.OnLoadedHandler -= OnInitialized;

        minimapActiveToggle.onValueChanged.RemoveAllListeners();
        projectionToggle.onValueChanged.RemoveAllListeners();
        levelSlider.onValueChanged.RemoveAllListeners();
    }
    private void OnInitialized(object sender, LoadDataEventArgs e)
    {
        Debug.Log("MinimapController에서 이벤트핸들러 실행");
        SetPostion(e.item.transform);
    }

    //미니맵 사용 시 주석 해제
    //private void Update()
    //{
    //    if (playerPoint)
    //    {
    //        Vector3 pos = playerPoint.position;
    //        pos.x = Camera.main.transform.position.x;
    //        pos.z = Camera.main.transform.position.z;
    //        playerPoint.position = Vector3.Lerp(playerPoint.position, pos, 0.5f);
    //    }
    //    if (playerRotateMarker)
    //    {
    //        Vector3 rot = playerRotateMarker.transform.localEulerAngles;
    //        rot.z = -Camera.main.transform.eulerAngles.y;
    //        playerRotateMarker.transform.localEulerAngles = rot;
    //    }
    //}
    void SetPostion(Transform target)
    {
        Bounds targetBound = ModelModifier.RendererBound(target);
        Vector3 targetCenter = targetBound.center;
        targetCenter.y = targetBound.max.y + 5f;

       minimapCam.transform.position = targetCenter;
    }
    void OnValueChanged_ActiveToggle(Toggle toggle)
    {
        Minimap.gameObject.SetActive(toggle.isOn);
    }

    void OnValueChanged_MapToggle(Slider slider)
    {
        Vector3 position = minimapCam.transform.position;
        position.y = slider.value;
        minimapCam.transform.position = position;
    }

    void OnValueChanged_ProjectionToggle(Toggle toggle)
    {
        if (toggle.isOn)
        {
            minimapCam.orthographic = true;
            levelSlider.value = 20;
            levelSlider.minValue = OrthographMinValue;
            levelSlider.maxValue = OrthographMaxValue;
        }
        else
        {
            minimapCam.orthographic = false;
            levelSlider.value = 100;
            levelSlider.minValue = PerspectiveMinValue;
            levelSlider.maxValue = PerspectiveMaxValue;
        }
    }
    #endregion
}