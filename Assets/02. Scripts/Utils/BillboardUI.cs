using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    Camera targetCam;
    bool isReverse;

    private void Awake()
    {
        targetCam = Camera.main;
    }

    private void Update()
    {
        transform.LookAt(targetCam.transform);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + (isReverse ? 180f : 0f), 0);
    }
}