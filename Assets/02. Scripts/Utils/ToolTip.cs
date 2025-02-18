using UnityEngine;
using UnityEngine.EventSystems;

public class ToolTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public string context;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Tooltip(true, transform, context);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Tooltip(false, transform);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Tooltip(false, transform);
    }

    public void Tooltip(bool value, Transform target, string context = null)
    {
        Canvas tooltipCanvas = GetComponentInParent<Canvas>();

        if (context != null)
            UILayoutManager.Instance.tooltip.GetComponentInChildren<TMPro.TMP_Text>().text = context;
        Vector3 targetPosition = target.GetComponent<RectTransform>().position;
        targetPosition.x += 20f * tooltipCanvas.scaleFactor;
        targetPosition.y -= 30f * tooltipCanvas.scaleFactor;
        UILayoutManager.Instance.tooltip.transform.position = targetPosition;
        UILayoutManager.Instance.tooltip.SetActive(value);
    }
}
