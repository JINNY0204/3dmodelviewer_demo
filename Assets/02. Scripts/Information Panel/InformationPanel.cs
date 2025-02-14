using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;

public class InformationPanel : MonoBehaviour
{
    public bool isFixed;
    public bool showOnAwake;
    public GameObject panel;
    public GameObject draggbleBar;
    public Button activeBtn;
    public Toggle fixLayoutToggle;
    public Transform content;
    Vector3 originToggleRot;
    [Header("Data Prefab")]
    public GameObject Category;
    public GameObject Cell;
    [Header("Load Data")]
    public TMP_Text TagName;
    public TMP_Text SubName;
    public TumbnailCapture thumbnail;

    private void Start()
    {
        ProcessManager.Instance.OnLoadedHandler += OnInitialized;
        ProcessManager.Instance.OnReleasedHandler += OnReleased;

        originToggleRot = fixLayoutToggle.transform.localEulerAngles;
        panel.SetActive(showOnAwake);

        fixLayoutToggle.isOn = isFixed;
    }
    #region EventHandler
    private void OnDestroy()
    {
        if (ProcessManager.Instance)
        {
            ProcessManager.Instance.OnLoadedHandler -= OnInitialized;
            ProcessManager.Instance.OnReleasedHandler -= OnReleased;
        }
    }

    public void OnInitialized(object sender, LoadDataEventArgs e)
    {
        //Debug.Log("InformationPanel에서 이벤트핸들러 실행", gameObject);
        //thumbnail.DrawTarget(e.item.transform);
    }
    private void OnReleased(object sender, Transform targetModel)
    {
        ReleaseData(targetModel, true);
    }
    #endregion
    public void On_fixLayoutToggle_ValueChanged()
    {
        isFixed = fixLayoutToggle.isOn;
        draggbleBar.SetActive(!isFixed);
        panel.GetComponent<DraggableWindow>().enabled = !isFixed;

        if (fixLayoutToggle.isOn)
        {
            fixLayoutToggle.transform.DOLocalRotate(Vector3.zero, 0.1f).SetDelay(0.1f);
        }
        else
        {
            fixLayoutToggle.transform.DOLocalRotate(originToggleRot, 0.1f).SetDelay(0.1f);
        }
    }
    public void OnClick_activeBtn()
    {
        panel.SetActive(!panel.activeSelf);

    }
    public void UpdateData(Transform dataTarget, bool showPanel = true)
    {
        thumbnail.DrawTarget(dataTarget);

        if (showPanel)
            panel.SetActive(true);
    }
    public void ReleaseData(Transform dataTarget, bool hidePanel = true)
    {
        thumbnail.ReleaseTarget(dataTarget);

        if (!fixLayoutToggle.isOn)
        {
            panel.SetActive(false);
        }
    }

    public void SetTagName(string tagName)
    {
        TagName.text = tagName;
    }

}