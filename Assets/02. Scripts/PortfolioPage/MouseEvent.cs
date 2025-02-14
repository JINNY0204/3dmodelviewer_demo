using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.Events;

public class MouseEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    protected UnityEvent OnClickEvent;
    protected UnityEvent OnEnterEvent;
    protected UnityEvent OnExitEvent;

    protected virtual void Awake()
    {
        RemoveEvents();
    }
    protected virtual void RemoveEvents()
    {
        OnClickEvent = new UnityEvent();
        OnEnterEvent = new UnityEvent();
        OnExitEvent = new UnityEvent();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClickEvent?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnEnterEvent?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnExitEvent?.Invoke();
    }
}
