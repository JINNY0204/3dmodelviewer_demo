using UnityEngine;

public class PageIndexItem : MouseEvent
{
    public TMPro.TMP_Text label;
    public TMPro.TMP_Text pageNumber;
    public GameObject selectionMark;
    public Color highlightColor;
    [HideInInspector] public Color originTextColor;
    private bool isSelected = false;  // 선택 상태 추가

    protected override void Awake()
    {
        base.Awake();
        OnEnterEvent.AddListener(OnEnter);
        OnExitEvent.AddListener(OnExit);
    }

    private void OnDestroy()
    {
        RemoveEvents();
    }

    private void OnEnter()
    {
        if (!isSelected)  // 선택되지 않은 상태일 때만 실행
        {
            label.color = Color.white;
            label.fontStyle = TMPro.FontStyles.Bold;
        }
        selectionMark.SetActive(true);
    }

    private void OnExit()
    {
        if (!isSelected)  // 선택되지 않은 상태일 때만 실행
        {
            label.color = originTextColor;
            label.fontStyle = TMPro.FontStyles.Normal;
        }
        selectionMark.SetActive(false);
    }

    public void Set()
    {
        originTextColor = label.color;
    }

    // 선택 상태 설정 메서드 추가
    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (selected)
        {
            // 선택된 상태의 스타일
            label.color = highlightColor;
            label.fontStyle = TMPro.FontStyles.Bold;
        }
        else
        {
            // 선택 해제 시 기본 스타일로
            label.color = originTextColor;
            label.fontStyle = TMPro.FontStyles.Normal;
        }
    }
}