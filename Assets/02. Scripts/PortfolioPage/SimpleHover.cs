using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using TMPro;

public class SimpleHover : MouseEvent
{
    [SerializeField] private float hoverHeight = 10f;  // 올라가는 높이
    [SerializeField] private float duration = 0f;    // 애니메이션 시간
    [SerializeField] private float delay = 0f;
    [SerializeField] private bool playOnAwake = false;
    private RectTransform rectTransform;
    private Vector2 originalAnchoredPosition;

    protected override void Awake()
    {
        base.Awake();

        TMP_Text tmpText = GetComponent<TMP_Text>();
        if (tmpText != null)
        {
            tmpText.raycastTarget = true;
        }
    }

    private void Start()
    {
        OnEnterEvent.AddListener(() =>
        {
            rectTransform.DOAnchorPos(
            originalAnchoredPosition + Vector2.up * hoverHeight,
            duration
        );});

        OnExitEvent.AddListener(() =>
        {
            rectTransform.DOAnchorPos(originalAnchoredPosition, duration);
        });

        rectTransform = GetComponent<RectTransform>();
        originalAnchoredPosition = rectTransform.anchoredPosition;

        if (playOnAwake)
        {
            DOTween.Kill(rectTransform);
            Sequence sequence = DOTween.Sequence();
        }
    }

    private void OnDestroy()
    {
        DOTween.Kill(rectTransform);
        RemoveEvents();
    }

    public void ResetPosition()
    {
        if (rectTransform)
        {
            rectTransform.anchoredPosition = originalAnchoredPosition;
        }
    }

    private void Reset()
    {
        TMP_Text tmpText = GetComponent<TMP_Text>();
        if (tmpText != null)
        {
            tmpText.raycastTarget = true;
        }
    }
}