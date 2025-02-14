using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HelpManager : MonoBehaviour
{
    public Button helpBtn;
    public GameObject helpPanel;
    [Header("sub contents")]
    public GameObject[] subContents;
    public Button arrowLeft;
    public Button arrowRight;
    GameObject currentPageObj;
    int currentPage = 1;
    int totalPage = 0;
    
    private void Awake()
    {
        if (arrowLeft)
            arrowLeft.GetComponent<Button>().onClick.AddListener(delegate { OnClick_ArrowLeft(); });
        if (arrowRight)
            arrowRight.GetComponent<Button>().onClick.AddListener(delegate { OnClick_ArrowRight(); });
        totalPage = subContents.Length;


        foreach (var item in GetComponentsInChildren<HelpItem>())
        {
            item.SetTransform();
        }
    }
    private void OnEnable()
    {
        foreach (var item in helpPanel.GetComponentsInChildren<Image>()) 
        {
            float originAlpha = item.color.a;
            Color color = item.color;
            color.a = 0f;
            item.color = color;

            item.DOFade(originAlpha, 0.4f);
        }
    }

    bool prevToggleState;
    public void OnClick_HelpBtn()
    {
        helpPanel.SetActive(true);
        prevToggleState = UILayoutManager.Instance.flipToggle.isOn;
        if (!prevToggleState)
            UILayoutManager.Instance.flipToggle.isOn = true;
        //    UILayoutManager.Instance.ShowToolbar();
    }
    public void OnClick_CloseBtn()
    {
        if (!UILayoutManager.Instance.isTweening)
        {
            helpPanel.SetActive(false);
            UILayoutManager.Instance.flipToggle.isOn = prevToggleState;
        }
    }

    #region paging
    void OnClick_ArrowLeft()
    {
        arrowRight.interactable = true;
        if (currentPage == 1) return;
        currentPage -= 1;

        FlipTo(currentPage);
    }
    void OnClick_ArrowRight()
    {
        arrowLeft.interactable = true;
        if (currentPage == totalPage) return;
        currentPage++;

        FlipTo(currentPage);
    }

    public void FlipTo(int page)
    {
        if (currentPageObj)
            currentPageObj.SetActive(false);
        else subContents[0].SetActive(false);

        subContents[page - 1].SetActive(true);
        currentPageObj = subContents[page - 1];

        if (page == 1) //첫 페이지라면,
        {
            arrowLeft.gameObject.SetActive(false);
            arrowRight.gameObject.SetActive(true);
        }
        else if (page == totalPage) //마지막 페이지라면,
        {
            arrowLeft.gameObject.SetActive(true);
            arrowRight.gameObject.SetActive(false);
        }
        else
        {
            arrowLeft.gameObject.SetActive(true);
            arrowRight.gameObject.SetActive(true);
        }
        currentPage = page;
    }
    #endregion
}