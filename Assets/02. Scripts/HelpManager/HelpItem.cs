using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class HelpItem : MonoBehaviour
{
    public RectTransform targetGUI;
    public Vector2 offset = Vector2.zero;
    Transform originalParent;

    public void SetTransform()
    {
        originalParent = targetGUI.parent;
        Canvas targetCanvas = targetGUI.GetComponentInParent<Canvas>();
        targetGUI.SetParent(targetCanvas.transform);

        RectTransform rect = GetComponent<RectTransform>();
        rect.anchorMin = targetGUI.anchorMin;
        rect.anchorMax = targetGUI.anchorMax;
        rect.sizeDelta = targetGUI.sizeDelta;
        rect.offsetMin = targetGUI.offsetMin;
        rect.offsetMax = targetGUI.offsetMax;

        Vector2 pos = targetGUI.anchoredPosition;
        pos.x += offset.x;
        pos.y += offset.y; 

        rect.anchoredPosition = pos;
        rect.rotation = targetGUI.rotation;
        rect.localScale = targetGUI.localScale;

        targetGUI.SetParent(originalParent);
    }
}