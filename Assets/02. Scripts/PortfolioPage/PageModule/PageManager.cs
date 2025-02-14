using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PageManager : MonoBehaviour
{
    #region Private Fields
    private List<PageGroupData> pageGroups = new List<PageGroupData>();
    private Dictionary<PageGroup, PageGroupData> groupDataMap = new Dictionary<PageGroup, PageGroupData>();
    private Dictionary<PageGroup, int> groupIndexMap = new Dictionary<PageGroup, int>();
    private Dictionary<Page, int> pageIndexMap = new Dictionary<Page, int>();

    private int totalPage;
    private int currentPage;
    private int currentGroupIndex;
    private int currentPageInGroup;

    // 캐시된 컴포넌트들
    private Button nextPageButtonComponent;
    private SimpleHover nextPageButtonHover;

    private class PageGroupData
    {
        public PageGroup group;
        public List<Page> pages = new List<Page>();
    }
    #endregion

    #region Public Fields
    public ContentsTable contentsTable;
    public int CurrentPage => currentPage;

    [Header("머리말/꼬리말")]
    public bool hideInFirstPage;
    public TMPro.TMP_Text header;
    public TMPro.TMP_Text pageNo;

    public enum TransitionEffect { NONE, FADE, SLIDE }
    [Header("페이지 전환 효과")]
    public TransitionEffect transitionEffect;
    [Range(0, 1)] public float slideDuration = 0.5f;
    [Range(0, 1)] public float fadeDuration = 0.5f;

    [Header("버튼 설정")]
    public GameObject pageButtonsGroup;
    public GameObject nextPageButton;
    #endregion

    private void Start()
    {
        DOTween.Init();
        CacheComponents();
        RegisterPages();
        InitializePages();
        contentsTable?.GenerateTable();
        ResetAllPages();
        UpdateCurrentPageInContents(-1, currentPage);
    }

    public List<(string title, List<Page> pages)> GetPageGroups()
    {
        return pageGroups.Select(g => (g.group.title, g.pages)).ToList();
    }

    #region Initialization
    private void CacheComponents()
    {
        if (nextPageButton != null)
        {
            nextPageButtonComponent = nextPageButton.GetComponent<Button>();
            nextPageButtonHover = nextPageButton.GetComponent<SimpleHover>();
        }
    }

    private void InitializePages()
    {
        currentPage = 0;
        currentGroupIndex = 0;
        currentPageInGroup = 0;
        totalPage = pageGroups.Sum(g => g?.pages?.Count ?? 0);

        UpdateUIStates();
    }

    private void ResetAllPages()
    {
        if (pageGroups == null || pageGroups.Count == 0)
        {
            SetMinimalUIState();
            return;
        }

        try
        {
            DisableAllPages();
            EnableFirstValidPage();
            currentPage = 0;
            UpdateUIStates();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in ResetAllPages: {e.Message}\n{e.StackTrace}");
            RecoverFromError();
        }
    }

    private void SetMinimalUIState()
    {
        if (pageButtonsGroup != null)
            pageButtonsGroup.SetActive(false);

        if (nextPageButtonComponent != null)
            nextPageButtonComponent.interactable = false;
        if (nextPageButtonHover != null)
            nextPageButtonHover.enabled = false;

        if (hideInFirstPage)
        {
            if (header != null) header.gameObject.SetActive(false);
            if (pageNo != null) pageNo.gameObject.SetActive(false);
        }
    }

    private void DisableAllPages()
    {
        foreach (var groupData in pageGroups)
        {
            groupData.group?.gameObject.SetActive(false);
            if (groupData.pages != null)
            {
                foreach (var page in groupData.pages)
                {
                    page?.gameObject.SetActive(false);
                }
            }
        }
    }

    private void EnableFirstValidPage()
    {
        for (int i = 0; i < pageGroups.Count; i++)
        {
            var group = pageGroups[i];
            if (group?.group != null && group.pages != null && group.pages.Count > 0)
            {
                currentGroupIndex = i;
                currentPageInGroup = 0;

                group.group.gameObject.SetActive(true);
                group.pages[0].gameObject.SetActive(true);
                break;
            }
        }
    }

    private void RecoverFromError()
    {
        currentPage = 0;
        currentGroupIndex = 0;
        currentPageInGroup = 0;

        if (pageGroups.Count > 0 && pageGroups[0]?.pages?.Count > 0)
        {
            pageGroups[0].group?.gameObject.SetActive(true);
            pageGroups[0].pages[0]?.gameObject.SetActive(true);
        }
    }
    #endregion

    #region Page Registration
    public int GetPageIndexForPage(Page page)
    {
        int globalPageIndex = 0;
        foreach (var group in pageGroups)
        {
            foreach (var p in group.pages)
            {
                if (p == page)
                {
                    return globalPageIndex;
                }
                globalPageIndex++;
            }
        }
        return -1; // 페이지를 찾지 못한 경우
    }
    private void RegisterPages()
    {
        var groups = GetComponentsInChildren<PageGroup>(true)
                    .OrderBy(g => g.transform.GetSiblingIndex());

        int globalPageIndex = 0;
        int groupIndex = 0;

        foreach (var group in groups)
        {
            var pages = group.GetComponentsInChildren<Page>(true)
                       .OrderBy(p => p.transform.GetSiblingIndex());

            if (!groupDataMap.TryGetValue(group, out PageGroupData groupData))
            {
                groupData = new PageGroupData { group = group };
                groupDataMap[group] = groupData;
                pageGroups.Add(groupData);
            }

            groupData.pages = new List<Page>(pages);

            // 하위 페이지 수에 따라 그룹 인덱스 설정
            if (pages.Count() == 1)
            {
                // 하위 페이지가 1개면 그 페이지의 인덱스와 동일
                groupIndexMap[group] = globalPageIndex;
                pageIndexMap[pages.First()] = globalPageIndex;
                globalPageIndex++;
            }
            else if (pages.Count() > 1)
            {
                // 하위 페이지가 2개 이상이면 첫 번째 하위 페이지의 인덱스와 동일
                groupIndexMap[group] = globalPageIndex;

                // 각 페이지에 글로벌 인덱스 부여
                foreach (var page in pages)
                {
                    pageIndexMap[page] = globalPageIndex;
                    globalPageIndex++;
                }
            }

            groupIndex++;
        }
    }

    public void UnregisterPage(Page page)
    {
        foreach (var groupData in pageGroups.ToArray())
        {
            if (groupData.pages.Remove(page) && groupData.pages.Count == 0)
            {
                pageGroups.Remove(groupData);
                groupDataMap.Remove(groupData.group);
                break;
            }
        }
    }
    #endregion

    #region UI Updates
    private void UpdateUIStates()
    {
        if (pageButtonsGroup != null)
            pageButtonsGroup.SetActive(currentPage != 0);

        if (nextPageButtonComponent != null)
            nextPageButtonComponent.interactable = currentPage < totalPage - 1;
        if (nextPageButtonHover != null)
            nextPageButtonHover.enabled = currentPage < totalPage - 1;

        bool isFirstPage = currentPage == 0;
        if (hideInFirstPage)
        {
            if (header != null) header.gameObject.SetActive(!isFirstPage);
            if (pageNo != null) pageNo.gameObject.SetActive(!isFirstPage);
        }

        if (header != null && currentGroupIndex < pageGroups.Count)
        {
            header.text = pageGroups[currentGroupIndex].group.subTitle ?? "";
        }

        if (pageNo != null)
        {
            pageNo.text = $"{currentPage + 1} / {totalPage}";
        }
    }

    private void UpdateCurrentPageInContents(int prevPage, int newPage)
    {
        if (contentsTable == null) return;

        if (prevPage >= 0)
            contentsTable.Release(prevPage);

        if (newPage >= 0)
            contentsTable.Select(newPage);
    }
    #endregion

    #region Navigation
    public void NavigateToPage(int targetPage, bool withEffect = true)
    {
        if (!IsValidPageNumber(targetPage) || targetPage == currentPage)
            return;
        if (currentTransitionSequence != null && currentTransitionSequence.IsPlaying())
            return;


        int prevPage = currentPage;
        GameObject fromPage = GetCurrentPageObject();

        UpdatePageIndices(targetPage);

        GameObject toPage = ActivateTargetPage();
        currentPage = targetPage;

        if (withEffect)
            PlayPageTransitionEffect(fromPage, toPage, prevPage < targetPage);

        UpdateCurrentPageInContents(prevPage, targetPage);
        UpdateUIStates();
    }

    private bool IsValidPageNumber(int pageNumber)
    {
        if (pageNumber < 0 || pageNumber >= totalPage || pageGroups == null || pageGroups.Count == 0)
        {
            Debug.LogWarning($"Invalid page number: {pageNumber}. Valid range is 0-{totalPage - 1}");
            return false;
        }
        return true;
    }

    private GameObject GetCurrentPageObject()
    {
        if (currentGroupIndex >= pageGroups.Count ||
            currentPageInGroup >= pageGroups[currentGroupIndex].pages.Count)
            return null;

        var currentGroup = pageGroups[currentGroupIndex];
        var currentPage = currentGroup.pages[currentPageInGroup];
        currentPage?.gameObject.SetActive(false);
        currentGroup.group?.gameObject.SetActive(false);

        return currentPage?.gameObject;
    }

    private void UpdatePageIndices(int targetPage)
    {
        int accumulatedPages = 0;
        for (int i = 0; i < pageGroups.Count; i++)
        {
            int groupPageCount = pageGroups[i].pages.Count;
            if (targetPage < accumulatedPages + groupPageCount)
            {
                currentGroupIndex = i;
                currentPageInGroup = targetPage - accumulatedPages;
                return;
            }
            accumulatedPages += groupPageCount;
        }
    }

    private GameObject ActivateTargetPage()
    {
        var newGroup = pageGroups[currentGroupIndex];
        newGroup.group?.gameObject.SetActive(true);

        var targetPage = newGroup.pages[currentPageInGroup];
        targetPage?.gameObject.SetActive(true);

        return targetPage?.gameObject;
    }

    public void OnNextPage() => NavigateToPage(currentPage + 1);
    public void OnPrevPage() => NavigateToPage(currentPage - 1);
    public void OnFirstPage() => NavigateToPage(0);
    public void OnLastPage() => NavigateToPage(totalPage - 1);
    public void ResetToFirstPage() => ResetAllPages();
    #endregion

    #region Effects
    private Sequence currentTransitionSequence = null;
    private void PlayPageTransitionEffect(GameObject currentPageObj, GameObject nextPageObj, bool isNext = true, TweenCallback onComplete = null)
    {
        if (transitionEffect == TransitionEffect.NONE || currentPageObj == null || nextPageObj == null)
        {
            onComplete?.Invoke();
            return;
        }

        DOTween.Kill(currentPageObj);
        DOTween.Kill(nextPageObj);
        currentTransitionSequence?.Kill();
        currentTransitionSequence = DOTween.Sequence();

        var currentCanvasGroup = currentPageObj.GetComponent<CanvasGroup>();
        if (currentCanvasGroup == null) currentCanvasGroup = currentPageObj.AddComponent<CanvasGroup>();
        currentCanvasGroup.alpha = 1f;  // 시작할 때 현재 페이지 배경은 보이도록 설정
        currentCanvasGroup.gameObject.SetActive(true);

        var nextCanvasGroup = nextPageObj.GetComponent<CanvasGroup>();
        if (nextCanvasGroup == null) nextCanvasGroup = nextPageObj.AddComponent<CanvasGroup>();
        nextCanvasGroup.alpha = 0f;
        nextPageObj.SetActive(true);

        currentCanvasGroup.DOFade(0f, fadeDuration);
        nextCanvasGroup.DOFade(1f, fadeDuration / 2).OnComplete(() =>
        {
            if (transitionEffect == TransitionEffect.FADE || currentPage == 0)
            {
                currentPageObj.SetActive(false);
                onComplete?.Invoke();
            }
        });

        if (transitionEffect == TransitionEffect.SLIDE)
        {
            float screenWidth = ((RectTransform)currentPageObj.transform).rect.width;
            float direction = isNext ? 1 : -1;
            var nextRect = ((RectTransform)nextPageObj.transform);
            nextRect.anchoredPosition = new Vector2(screenWidth * 0.2f * direction, 0f);

            // 현재 페이지는 위/아래로 슬라이드
            currentTransitionSequence.Append(((RectTransform)currentPageObj.transform)
                .DOAnchorPosX(screenWidth * -direction, slideDuration)
                .SetEase(Ease.InOutQuad));

            // 다음 페이지는 반대쪽에서 슬라이드
            currentTransitionSequence.Join(nextRect
                .DOAnchorPosX(0, slideDuration)
                .SetEase(Ease.InOutQuad));

            currentTransitionSequence.OnComplete(() => {
                currentPageObj.SetActive(false);
                onComplete?.Invoke();
            });
        }
    }
    #endregion
}