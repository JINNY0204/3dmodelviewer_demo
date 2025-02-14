using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ZoomController : MonoBehaviour
{
    CameraController camController;
    public float zoomSpeed;
    public float depth;
    bool isZooming;

    private void Awake()
    {
        camController = Camera.main.GetComponent<CameraController>();
    }

    public void ZoomIn()
    {
        camController.cameraTransform.DOKill();
        camController.cameraTransform.DOMove(camController.cameraTransform.position + camController.cameraTransform.forward * depth, zoomSpeed);
    }

    public void ZoomOut()
    {
        camController.cameraTransform.DOKill();
        camController.cameraTransform.DOMove(camController.cameraTransform.position - camController.cameraTransform.forward * depth, zoomSpeed);
    }
}
