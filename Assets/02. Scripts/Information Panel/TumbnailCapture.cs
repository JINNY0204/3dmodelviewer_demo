using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TumbnailCapture : MonoBehaviour
{
    public enum Direction { Top, Front, Back, Side, Quater }
    public Direction direction;
    public Transform camPivot;
    public Transform originalParent;
    public Camera targetCam;
    public RenderTexture targetRT;

    void FocusOnTarget(Transform target)
    {
        Bounds bounds = ModelModifier.RendererBound(target);
        camPivot.transform.position = bounds.center;

        Vector3 modelPivotRotation = Vector3.zero;
        Vector3 camPos = Vector3.zero;
        float distance = (Camera.main.transform.position - bounds.center).magnitude;
        distance = Mathf.Clamp(distance, (bounds.center - bounds.max).magnitude * 1.5f, (bounds.center - bounds.max).magnitude * 1.5f);
        switch (direction)
        {
            case Direction.Top:
                modelPivotRotation.x = 80f;
                break;
            case Direction.Front:
                modelPivotRotation = Vector3.zero;
                break;
            case Direction.Back:
                modelPivotRotation.y = 180f;
                break;
            case Direction.Side:
                modelPivotRotation.y = 90f;
                break;
            case Direction.Quater:
                modelPivotRotation.x = 30f;
                modelPivotRotation.y = 30f;
                break;
            default:
                break;
        }

        camPos.z -= distance;
        camPivot.eulerAngles = modelPivotRotation;
        camPivot.SetParent(target);

        targetCam.transform.localEulerAngles = Vector3.zero;
        targetCam.transform.localPosition = camPos;
    }

    public void DrawTarget(Transform target)
    {
        FocusOnTarget(target);
        Transform[] targetArray = target.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < targetArray.Length; i++)
        {
            targetArray[i].gameObject.layer = LayerMask.NameToLayer("Selected");
        }
    }

    public void ReleaseTarget(Transform target)
    {
        if (target)
        {
            camPivot.SetParent(originalParent);
            Transform[] targetArray = target.GetComponentsInChildren<Transform>(true);

            for (int i = 0; i < targetArray.Length; i++)
                targetArray[i].gameObject.layer = LayerMask.NameToLayer("Default");
            targetRT.Release();
        }
    }
}