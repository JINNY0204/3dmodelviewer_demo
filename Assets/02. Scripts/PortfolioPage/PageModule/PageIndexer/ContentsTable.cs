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
        // 기존 아이템 초기화
        foreach (var item in contentItems)
        {
            Destroy(item);
        }
        contentItems.Clear();

        var groups = pageManager.GetPageGroups();

        foreach (var group in groups)
        {
            // 그룹 제목 생성
            GameObject groupItem = CreateContentItem(group.title, groupIndent, -1, true);
            Button groupButton = groupItem.GetComponent<Button>();

            // 그룹의 페이지 수에 따라 다른 로직
            var pages = group.pages;
            if (pages.Count == 1)
            {
                // 페이지가 1개뿐이면 그룹 자체를 클릭 가능하게 설정
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
                // 하위 페이지가 2개 이상이면 그룹 버튼 비활성화
                if (groupButton != null)
                {
                    groupButton.interactable = false;
                    groupButton.GetComponent<PageIndexItem>().enabled = false;
                }

                // 그룹 내 페이지들 생성
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

        // 그룹이 아닌 경우에만 버튼 이벤트 설정
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
                // RectTransform을 사용해 스크롤뷰 내에서 아이템 위치 계산
                RectTransform contentRect = contentParent.GetComponent<RectTransform>();
                RectTransform targetRect = targetItem.GetComponent<RectTransform>();
                ScrollRect scrollRect = GetComponent<ScrollRect>();

                if (scrollRect != null)
                {
                    // 아이템의 상대적 위치 계산
                    Vector2 itemPositionInContent = contentRect.InverseTransformPoint(targetRect.position);
                    Vector2 maskPositionInContent = contentRect.InverseTransformPoint(scrollRect.viewport.position);

                    // 스크롤 위치 계산
                    float normalizedPosition = Mathf.Clamp01(
                        (contentRect.rect.height - itemPositionInContent.y) /
                        (contentRect.rect.height - scrollRect.viewport.rect.height)
                    );

                    // 부드러운 스크롤
                    scrollRect.verticalNormalizedPosition = normalizedPosition;
                }
            }
        }
    }
}