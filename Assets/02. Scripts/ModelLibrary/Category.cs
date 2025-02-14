using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Category : MonoBehaviour
{
    public Toggle titlebarToggle;
    public Image foldIcon;
    public TMP_Text label;
    Vector3 originIconRot;

    public GameObject container;

    private void Awake()
    {
        originIconRot = foldIcon.transform.localEulerAngles;
    }
    public void Fold()
    {
        if (titlebarToggle.isOn)
        {
            container.SetActive(false);
            foldIcon.transform.DOLocalRotate(originIconRot + Vector3.forward * 180f, 0.1f).SetDelay(0.1f);
        }
        else
        {
            container.SetActive(true);
            foldIcon.transform.DOLocalRotate(originIconRot + Vector3.forward / 180f, 0.1f).SetDelay(0.1f);
        }
    }
}