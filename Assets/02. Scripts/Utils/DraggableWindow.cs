using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

public class DraggableWindow : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public bool dettachFromParent;
    public bool rememberPosition = true;
    public bool tweenEffect;
    private Vector2 pointerOffset;
    private Vector2 originPos;
    private RectTransform canvasRectTransform;
    private RectTransform panelRectTransform;
    private bool clampedToLeft;
    private bool clampedToRight;
    private bool clampedToTop;
    private bool clampedToBottom;
    private Color originColor;
    private Canvas canvas;
    private Transform originParent;

    void OnEnable()
    {

    }

    void OnDisable()
    {
        if (!rememberPosition)
            panelRectTransform.anchoredPosition = originPos;
    }

    public void Awake()
    {
        originParent = transform.parent;
        canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvasRectTransform = canvas.transform as RectTransform;
            panelRectTransform = transform as RectTransform;
            originPos = panelRectTransform.anchoredPosition;
            if (panelRectTransform.GetComponent<Image>())
                originColor = panelRectTransform.GetComponent<Image>().color;
        }
        clampedToLeft = false;
        clampedToRight = false;
        clampedToTop = false;
        clampedToBottom = false;
    }

    #region IBeginDragHandler implementation
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (dettachFromParent)
            transform.SetParent(canvas.transform);

        panelRectTransform.SetAsLastSibling();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRectTransform, eventData.position, eventData.pressEventCamera, out pointerOffset);

        if (tweenEffect)
        {
            if (panelRectTransform.GetComponent<Image>())
            {
                Color panelColor = panelRectTransform.GetComponent<Image>().color;
                panelColor.a *= 0.8f;
                panelRectTransform.GetComponent<Image>().color = panelColor;
                panelRectTransform.DOScale(1.02f, 0.1f).SetEase(Ease.InFlash);
            }
        }
    }
    #endregion

    #region IDragHandler implementation
    public void OnDrag(PointerEventData eventData)
    {
        if (panelRectTransform == null)
        {
            return;
        }
        Vector2 localPointerPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, eventData.position, eventData.pressEventCamera, out localPointerPosition))
        {
            panelRectTransform.localPosition = localPointerPosition - pointerOffset;
            ClampToWindow();
            Vector2 clampedPosition = panelRectTransform.localPosition;
            if (clampedToRight)
            {
                clampedPosition.x = (canvasRectTransform.rect.width * 0.5f) - (panelRectTransform.rect.width * (1 - panelRectTransform.pivot.x));
            }
            else if (clampedToLeft)
            {
                clampedPosition.x = (-canvasRectTransform.rect.width * 0.5f) + (panelRectTransform.rect.width * panelRectTransform.pivot.x);
            }

            if (clampedToTop)
            {
                clampedPosition.y = (canvasRectTransform.rect.height * 0.5f) - (panelRectTransform.rect.height * (1 - panelRectTransform.pivot.y));
            }
            else if (clampedToBottom)
            {
                clampedPosition.y = (-canvasRectTransform.rect.height * 0.5f) + (panelRectTransform.rect.height * panelRectTransform.pivot.y);
            }
            panelRectTransform.localPosition = clampedPosition;
        }
    }

    #endregion

    #region IEndDragHandler implementation

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dettachFromParent)
            transform.SetParent(originParent, true);

        if (tweenEffect)
        {
            if (panelRectTransform.GetComponent<Image>())
            {
                panelRectTransform.GetComponent<Image>().color = originColor;
                panelRectTransform.DOScale(1f, 0.1f).SetEase(Ease.InExpo);
            }
        }
    }

    #endregion

    void ClampToWindow()
    {
        Vector3[] canvasCorners = new Vector3[4];
        Vector3[] panelRectCorners = new Vector3[4];
        canvasRectTransform.GetWorldCorners(canvasCorners);
        panelRectTransform.GetWorldCorners(panelRectCorners);

        if (panelRectCorners[2].x > canvasCorners[2].x)
        {
            if (!clampedToRight)
            {
                clampedToRight = true;
            }
        }
        else if (clampedToRight)
        {
            clampedToRight = false;
        }
        else if (panelRectCorners[0].x < canvasCorners[0].x)
        {
            if (!clampedToLeft)
            {
                clampedToLeft = true;
            }
        }
        else if (clampedToLeft)
        {
            clampedToLeft = false;
        }

        if (panelRectCorners[2].y > canvasCorners[2].y)
        {
            if (!clampedToTop)
            {
                clampedToTop = true;
            }
        }
        else if (clampedToTop)
        {
            clampedToTop = false;
        }
        else if (panelRectCorners[0].y < canvasCorners[0].y)
        {
            if (!clampedToBottom)
            {
                clampedToBottom = true;
            }
        }
        else if (clampedToBottom)
        {
            clampedToBottom = false;
        }
    }
}
