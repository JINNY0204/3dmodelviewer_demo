using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TriggerEventPopup : MouseEvent
{
    [SerializeField] private GameObject targetPanel;
    [SerializeField] private CanvasGroup panelCanvasGroup;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float targetAlpha = 0.9f;
    [SerializeField] private Vector2 moveAmount = Vector2.zero;
    private Vector2 originPosition;

    protected override void Awake()
    {
        base.Awake();

        originPosition = targetPanel.GetComponent<RectTransform>().anchoredPosition;
        //이벤트 추가
        OnEnterEvent.AddListener(() =>
        {
            // 패널 활성화 및 페이드 인
            targetPanel.SetActive(true);
            panelCanvasGroup.DOFade(targetAlpha, fadeDuration)
                .SetEase(Ease.OutSine);

            MoveRect(targetPanel.GetComponent<RectTransform>(), moveAmount * -1);
        });
        OnExitEvent.AddListener(() =>
        {
            // 패널이 마우스를 벗어나는 것을 감지하는 로직 추가
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                targetPanel.GetComponent<RectTransform>(),
                Input.mousePosition))
            {
                // 페이드 아웃 후 패널 비활성화
                panelCanvasGroup.DOFade(0f, fadeDuration)
                    .SetEase(Ease.OutSine)
                    .OnComplete(() => {
                        targetPanel.SetActive(false);
                    });

                MoveRect(targetPanel.GetComponent<RectTransform>(), moveAmount);
            }
        });

        // CanvasGroup 컴포넌트가 없다면 자동으로 추가
        if (panelCanvasGroup == null)
        {
            panelCanvasGroup = targetPanel.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = targetPanel.AddComponent<CanvasGroup>();
            }
        }

        // 초기 상태 설정
        targetPanel.SetActive(false);
        panelCanvasGroup.alpha = 0f;
    }


    // 패널에 EventTrigger 컴포넌트 추가 필요
    private void Start()
    {
        // 패널에 PointerExit 이벤트 추가
        EventTrigger eventTrigger = targetPanel.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = targetPanel.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => { OnPanelPointerExit(); });
        eventTrigger.triggers.Add(exitEntry);
    }

    private void OnPanelPointerExit()
    {
        // 페이드 아웃 후 패널 비활성화
        panelCanvasGroup.DOFade(0f, fadeDuration)
            .SetEase(Ease.OutSine)
            .OnComplete(() => {
                targetPanel.SetActive(false);
            });
    }

    void MoveRect(RectTransform targetRect, Vector2 moveAmount)
    {
        targetRect.DOKill();
        RectTransform canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        // 화면 크기 비율에 따라 이동량 계산
        float referenceAspectRatio = canvasRect.rect.width / canvasRect.rect.height;
        float currentAspectRatio = (float)Screen.width / (float)Screen.height;
        float aspectRatio = currentAspectRatio / referenceAspectRatio;
        // 현재 Canvas Scaler 모드에 따라 위치 조정
        targetRect.anchoredPosition = originPosition;
        Vector2 endValue = targetRect.anchoredPosition + moveAmount * aspectRatio;
        targetRect.DOAnchorPos(endValue, 0.2f);
    }
}
