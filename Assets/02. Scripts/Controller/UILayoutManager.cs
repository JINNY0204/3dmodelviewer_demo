using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using Unity.VisualScripting;

public class UILayoutManager : Singleton<UILayoutManager>
{
    [Header("구성요소")]
    public Canvas targetCanvas;
    public Transform mainToolBar;
    public Transform subToolBar;
    public InformationPanel detailInformation;
    public Toggle flipToggle;

    [Tooltip("TransparencySlider로 투명도를 조정할 대상을 넣으세요.")]
    public Image[] transparentTargets;

    [Header("기타 도구")]
    public GameObject tooltip;

    RectTransform mainToolBarRect;
    RectTransform subToolBarRect;

    Transform flipToggleIcon;
    Vector3 originFlipRot;
    [NonSerialized] public bool isTweening;

    private void Start()
    {
        ProcessManager.Instance.OnLoadedHandler += OnInitialized;
        ProcessManager.Instance.OnReleasedHandler += OnReleased;

        mainToolBarRect = mainToolBar.GetComponent<RectTransform>();
        subToolBarRect = subToolBar.GetComponent<RectTransform>();

        flipToggleIcon = flipToggle.transform.GetChild(0);
        originFlipRot = flipToggleIcon.localEulerAngles;
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
        flipToggle.isOn = true;
    }
    private void OnReleased(object sender, Transform targetModel)
    {

    }
    #endregion

    //MenuBar 접기, 펼치기
    public void Flip()
    {
        if (isTweening) return;

        if (flipToggle.isOn)
        {
            ShowToolbar();
        }
        else
        {
            HideToolbar();
        }
    }
    public void ShowToolbar()
    {
        isTweening = true;
        MoveRect(mainToolBarRect, new Vector2(0f, -mainToolBarRect.rect.height));
        MoveRect(subToolBarRect, new Vector2(-subToolBarRect.rect.width, 0f));
        flipToggleIcon.DOLocalRotate(originFlipRot + Vector3.forward * 180f, 0.2f).SetDelay(0.2f).OnComplete(delegate
        {
            isTweening = false;
        });
    }
    public void HideToolbar() 
    {
        isTweening = true;
        MoveRect(mainToolBarRect, new Vector2(0f, mainToolBarRect.rect.height));
        MoveRect(subToolBarRect, new Vector2(subToolBarRect.rect.width, 0f));
        flipToggleIcon.DOLocalRotate(originFlipRot + Vector3.forward / 180f, 0.2f).SetDelay(0.2f).OnComplete(delegate
        {
            isTweening = false;
        });
    }

    void MoveRect(RectTransform targetRect, Vector2 moveAmount)
    {
        RectTransform canvasRect = targetCanvas.GetComponent<RectTransform>();

        // 화면 크기 비율에 따라 이동량 계산
        float referenceAspectRatio = canvasRect.rect.width / canvasRect.rect.height;
        float currentAspectRatio = (float)Screen.width / (float)Screen.height;
        float aspectRatio = currentAspectRatio / referenceAspectRatio;
        // 현재 Canvas Scaler 모드에 따라 위치 조정
        Vector2 endValue = targetRect.anchoredPosition + moveAmount * aspectRatio;
        targetRect.DOAnchorPos(endValue, 0.2f).SetDelay(0.2f);
    }

    public void PanelTransparency(Slider slider)
    {
        for (int i = 0; i < transparentTargets.Length; i++)
        {
            Color color = transparentTargets[i].GetComponent<Image>().color;
            color.a = slider.value;
            transparentTargets[i].GetComponent<Image>().color = color;
        }
    }
}