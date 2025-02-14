using UnityEngine;

public class PageIndexItem : MouseEvent
{
    public TMPro.TMP_Text label;
    public TMPro.TMP_Text pageNumber;
    public GameObject selectionMark;
    public Color highlightColor;
    [HideInInspector] public Color originTextColor;
    private bool isSelected = false;  // ���� ���� �߰�

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
        if (!isSelected)  // ���õ��� ���� ������ ���� ����
        {
            label.color = Color.white;
            label.fontStyle = TMPro.FontStyles.Bold;
        }
        selectionMark.SetActive(true);
    }

    private void OnExit()
    {
        if (!isSelected)  // ���õ��� ���� ������ ���� ����
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

    // ���� ���� ���� �޼��� �߰�
    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (selected)
        {
            // ���õ� ������ ��Ÿ��
            label.color = highlightColor;
            label.fontStyle = TMPro.FontStyles.Bold;
        }
        else
        {
            // ���� ���� �� �⺻ ��Ÿ�Ϸ�
            label.color = originTextColor;
            label.fontStyle = TMPro.FontStyles.Normal;
        }
    }
}