using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class ViewModeController : MonoBehaviour
{
    public static ViewModeController Instance;
    public enum Direction { Top, Front, Back, Side, Quater, Full }
    public Direction direction;

    CameraController cameraController;
    public float farDistance = 100f;
    public float smoothness = 0.5f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        cameraController = Camera.main.GetComponent<CameraController>();
    }
    private void Start()
    {
        ProcessManager.Instance.OnLoadedHandler += OnInitialized;
    }
    private void OnDestroy()
    {
        if (ProcessManager.Instance)
            ProcessManager.Instance.OnLoadedHandler -= OnInitialized;
    }

    private void OnInitialized(object sender, LoadDataEventArgs e)
    {
        ChangeView(Direction.Front, e.item.transform);
    }

    #region OnClick Event
    public void OnClick_Top()
    {
        ChangeView(Direction.Top, ModelModifier.Instance.SelectedModel);
    }
    public void OnClick_Front()
    {
        ChangeView(Direction.Front, ModelModifier.Instance.SelectedModel);
    }
    public void OnClick_Back()
    {
        ChangeView(Direction.Back, ModelModifier.Instance.SelectedModel);
    }
    public void OnClick_Side()
    {
        ChangeView(Direction.Side, ModelModifier.Instance.SelectedModel);
    }
    public void OnClick_Quarter()
    {
        ChangeView(Direction.Quater, ModelModifier.Instance.SelectedModel);
    }

    public void OnClick_Full()
    {
        ChangeView(Direction.Full, ProcessManager.Instance.loadedModel);
    }
    #endregion

    public void ChangeView(Direction direction, Transform target)
    {
        if (target == null) return;
        this.direction = direction;
        //모델 피벗 위치 설정
        Bounds bounds = ModelModifier.RendererBound(target);
        cameraController.cameraPivot.position = bounds.center;

        //피벗 rotation 및 카메라 distace설정
        Vector3 modelPivotRotation = Vector3.zero;
        Vector3 camPos = Vector3.zero;
        //float distance = (Camera.main.transform.position - bounds.center).magnitude;
        float distance = (bounds.center - bounds.max).magnitude * 1.5f;
        distance = Mathf.Clamp(distance, cameraController.minDistance, (bounds.center - bounds.max).magnitude * 1.5f);


        switch (direction)
        {
            case Direction.Top:
                //distance = Mathf.Clamp(distance, bounds.size.y, bounds.size.y + farDistance);
                modelPivotRotation.x = 80f;
                camPos.z -= distance;
                break;
            case Direction.Front:
                //distance = Mathf.Clamp(distance, bounds.size.z, bounds.size.z + farDistance);
                modelPivotRotation = Vector3.zero;
                camPos.z -= distance;
                break;
            case Direction.Back:
                //distance = Mathf.Clamp(distance, bounds.size.z, bounds.size.z + farDistance);
                modelPivotRotation.y = 180f;
                camPos.z -= distance;
                break;
            case Direction.Side:
                //distance = Mathf.Clamp(distance, bounds.size.x, bounds.size.x + farDistance);
                modelPivotRotation.y = 90f;
                camPos.z -= distance;
                break;
            case Direction.Quater:
                //distance = Mathf.Clamp(distance, (bounds.center - bounds.max).magnitude, (bounds.center - bounds.max).magnitude);
                modelPivotRotation.x = 30f;
                modelPivotRotation.y = 30f;
                camPos.z -= distance;
                break;
            default:
                break;
        }

        //값 대입
        //CameraController.cameraPivot.eulerAngles = modelPivotRotation;
        Camera.main.transform.DOKill();
        cameraController.cameraPivot.DORotate(modelPivotRotation, smoothness);
        Camera.main.transform.DOLocalRotate(Vector3.zero, smoothness);
        Camera.main.transform.DOLocalMove(camPos, smoothness);
    }
}