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
        //�̺�Ʈ �߰�
        OnEnterEvent.AddListener(() =>
        {
            // �г� Ȱ��ȭ �� ���̵� ��
            targetPanel.SetActive(true);
            panelCanvasGroup.DOFade(targetAlpha, fadeDuration)
                .SetEase(Ease.OutSine);

            MoveRect(targetPanel.GetComponent<RectTransform>(), moveAmount * -1);
        });
        OnExitEvent.AddListener(() =>
        {
            // �г��� ���콺�� ����� ���� �����ϴ� ���� �߰�
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                targetPanel.GetComponent<RectTransform>(),
                Input.mousePosition))
            {
                // ���̵� �ƿ� �� �г� ��Ȱ��ȭ
                panelCanvasGroup.DOFade(0f, fadeDuration)
                    .SetEase(Ease.OutSine)
                    .OnComplete(() => {
                        targetPanel.SetActive(false);
                    });

                MoveRect(targetPanel.GetComponent<RectTransform>(), moveAmount);
            }
        });

        // CanvasGroup ������Ʈ�� ���ٸ� �ڵ����� �߰�
        if (panelCanvasGroup == null)
        {
            panelCanvasGroup = targetPanel.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = targetPanel.AddComponent<CanvasGroup>();
            }
        }

        // �ʱ� ���� ����
        targetPanel.SetActive(false);
        panelCanvasGroup.alpha = 0f;
    }


    // �гο� EventTrigger ������Ʈ �߰� �ʿ�
    private void Start()
    {
        // �гο� PointerExit �̺�Ʈ �߰�
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
        // ���̵� �ƿ� �� �г� ��Ȱ��ȭ
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

        // ȭ�� ũ�� ������ ���� �̵��� ���
        float referenceAspectRatio = canvasRect.rect.width / canvasRect.rect.height;
        float currentAspectRatio = (float)Screen.width / (float)Screen.height;
        float aspectRatio = currentAspectRatio / referenceAspectRatio;
        // ���� Canvas Scaler ��忡 ���� ��ġ ����
        targetRect.anchoredPosition = originPosition;
        Vector2 endValue = targetRect.anchoredPosition + moveAmount * aspectRatio;
        targetRect.DOAnchorPos(endValue, 0.2f);
    }
}
