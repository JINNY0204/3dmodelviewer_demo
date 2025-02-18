using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SizeDragger : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public Transform targetGUI;

    public float sensitivity;
    public Vector2 minMaxSize;
    public GameObject DraggerImage;
    public Image DragIcon;
    public Color normalColor;
    public Color draggingColor;
    public Color limitColor;

    bool isMouseOver = true;

    public void Initialize()
    {
        DraggerImage.SetActive(false);
        DragIcon.color = normalColor;

        Vector3 targetScale = targetGUI.GetComponent<RectTransform>().localScale;
        targetScale.x = minMaxSize.x;
        targetScale.y = minMaxSize.x;
        targetScale.z = minMaxSize.x;

        targetGUI.GetComponent<RectTransform>().localScale = targetScale;
    }

    void OnMouseOver()
    {
        isMouseOver = true;
    }
    void OnMouseExit()
    {
        isMouseOver = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isMouseOver)
        {
            DraggerImage.SetActive(true);

            float XDelta = Input.GetAxis("Mouse X");
            Vector3 targetScale = Vector3.one;
            if (targetGUI.GetComponent<RectTransform>().localScale.x >= minMaxSize.x
                && targetGUI.GetComponent<RectTransform>().localScale.x <= minMaxSize.y)
            {
                targetScale = targetGUI.GetComponent<RectTransform>().localScale;
                targetScale.x -= XDelta * sensitivity;
                targetScale.y -= XDelta * sensitivity;
                targetScale.z -= XDelta * sensitivity;

                targetGUI.GetComponent<RectTransform>().localScale = targetScale;
                DragIcon.color = draggingColor;
            }

            Vector3 reCollib = targetGUI.GetComponent<RectTransform>().localScale;
            if (targetGUI.GetComponent<RectTransform>().localScale.x <= minMaxSize.x)
            {
                reCollib.x = minMaxSize.x;
                reCollib.y = minMaxSize.x;
                reCollib.z = minMaxSize.x;

                DragIcon.color = limitColor;
            }
            if (targetGUI.GetComponent<RectTransform>().localScale.x >= minMaxSize.y)
            {
                reCollib.x = minMaxSize.y;
                reCollib.y = minMaxSize.y;
                reCollib.z = minMaxSize.y;

                DragIcon.color = limitColor;
            }
            targetGUI.GetComponent<RectTransform>().localScale = reCollib;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DraggerImage.SetActive(false);
        DragIcon.color = normalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        DraggerImage.SetActive(true);
        DragIcon.color = draggingColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        DraggerImage.SetActive(false);
        DragIcon.color = normalColor;
    }
}
