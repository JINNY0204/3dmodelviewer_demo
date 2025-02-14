using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

public class ContentsTable : MonoBehaviour
{
    [Header("References")]
    public PageManager pageManager;
    public GameObject contentItemPrefab;
    public Transform contentParent;
    public RectTransform tableRect;

    [Header("Styling")]
    public float groupIndent;
    public float pageIndent;

    private List<GameObject> contentItems = new List<GameObject>();


    public void GenerateTable()
    {
        // ���� ������ �ʱ�ȭ
        foreach (var item in contentItems)
        {
            Destroy(item);
        }
        contentItems.Clear();

        var groups = pageManager.GetPageGroups();

        foreach (var group in groups)
        {
            // �׷� ���� ����
            GameObject groupItem = CreateContentItem(group.title, groupIndent, -1, true);
            Button groupButton = groupItem.GetComponent<Button>();

            // �׷��� ������ ���� ���� �ٸ� ����
            var pages = group.pages;
            if (pages.Count == 1)
            {
                // �������� 1�����̸� �׷� ��ü�� Ŭ�� �����ϰ� ����
                int groupPageIndex = pageManager.GetPageIndexForPage(pages[0]);

                if (groupButton != null)
                {
                    groupButton.interactable = true;
                    groupButton.onClick.AddListener(() => pageManager.NavigateToPage(groupPageIndex));
                }


                contentItems.Add(groupItem);
            }
            else
            {
                // ���� �������� 2�� �̻��̸� �׷� ��ư ��Ȱ��ȭ
                if (groupButton != null)
                {
                    groupButton.interactable = false;
                    groupButton.GetComponent<PageIndexItem>().enabled = false;
                }

                // �׷� �� �������� ����
                int pageInGroupIndex = 1;
                foreach (var page in pages)
                {
                    int globalPageIndex = pageManager.GetPageIndexForPage(page);
                    string pageText = $"{pageInGroupIndex++}. {page.name}";

                    GameObject pageItem = CreateContentItem(pageText, pageIndent, globalPageIndex, false);

                    contentItems.Add(pageItem);
                }
            }
        }
    }

    public void Select(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < contentItems.Count)
        {
            var item = contentItems[pageIndex];
            if (item != null)
            {
                var indexItem = item.GetComponent<PageIndexItem>();
                if (indexItem != null)
                {
                    indexItem.SetSelected(true);
                    EnsureItemVisible(pageIndex);
                }
            }

        }
    }

    public void Release(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < contentItems.Count)
        {
            var item = contentItems[pageIndex];
            if (item != null)
            {
                var indexItem = item.GetComponent<PageIndexItem>();
                if (indexItem != null)
                {
                    indexItem.SetSelected(false);
                }
            }
        }
    }

    private GameObject CreateContentItem(string text, float indent, int pageIndex, bool isGroup)
    {
        GameObject item = Instantiate(contentItemPrefab, contentParent);
        item.gameObject.SetActive(true);
        
        RectTransform indentTargetRect = item.transform.GetChild(0).GetComponent<RectTransform>();
        Vector2 pos = indentTargetRect.anchoredPosition;
        pos.x += indent;
        indentTargetRect.anchoredPosition = pos;

        Vector2 size = indentTargetRect.sizeDelta;
        size.x -= indent;
        indentTargetRect.sizeDelta = size;

        PageIndexItem indexItem = item.GetComponent<PageIndexItem>();
        TMP_Text tmpText = indexItem.label;
        if (tmpText != null)
        {
            tmpText.text = text;
            //if (isGroup)
            //{
            //    //tmpText.fontStyle = FontStyles.Bold;
            //}
            indexItem.originTextColor = tmpText.color;
        }

        // �׷��� �ƴ� ��쿡�� ��ư �̺�Ʈ ����
        if (!isGroup && pageIndex >= 0)
        {
            Button btn = item.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => pageManager.NavigateToPage(pageIndex));
            }
        }

        return item;
    }

    public void EnsureItemVisible(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < contentItems.Count)
        {
            var targetItem = contentItems[pageIndex];
            if (targetItem != null)
            {
                // RectTransform�� ����� ��ũ�Ѻ� ������ ������ ��ġ ���
                RectTransform contentRect = contentParent.GetComponent<RectTransform>();
                RectTransform targetRect = targetItem.GetComponent<RectTransform>();
                ScrollRect scrollRect = GetComponent<ScrollRect>();

                if (scrollRect != null)
                {
                    // �������� ����� ��ġ ���
                    Vector2 itemPositionInContent = contentRect.InverseTransformPoint(targetRect.position);
                    Vector2 maskPositionInContent = contentRect.InverseTransformPoint(scrollRect.viewport.position);

                    // ��ũ�� ��ġ ���
                    float normalizedPosition = Mathf.Clamp01(
                        (contentRect.rect.height - itemPositionInContent.y) /
                        (contentRect.rect.height - scrollRect.viewport.rect.height)
                    );

                    // �ε巯�� ��ũ��
                    scrollRect.verticalNormalizedPosition = normalizedPosition;
                }
            }
        }
    }
}