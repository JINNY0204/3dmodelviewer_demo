using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ModelLibrary : MonoBehaviour
{
    Dictionary<Category, List<ItemCell>> categoryDic = new Dictionary<Category, List<ItemCell>>();
    List<ItemCell> itemCells = new List<ItemCell>();
    public UnityEvent onClickItem;
    public Transform content;
    public GameObject CategoryPrefab;
    public GameObject CellPrefab;
    public bool showTotalCount;
    private static ItemCell clickedItem;

    [Header("Sort")]
    public SearchInputField searchInput;

    [Header("Sort")]
    public Dropdown sortDropdown;

    [Header("Edit")]
    public Sprite noImage;
    public Toggle editToggle;
    public Button refreshBtn;
    public Image blockbar;
    Vector2 editToggleOriginpos;

    private void Start()
    {
        editToggleOriginpos = editToggle.GetComponent<RectTransform>().anchoredPosition;
    }

    public static ItemCell GetClickedItem()
    {
        return clickedItem;
    }

    public void Create(string label, Function OnClick = null)
    {
        itemCells = new List<ItemCell>();
        Category ca = AddCategory(label);

        AddressableAssetLoader.GetAllAddressableKeys(label, resultList =>
        {
            if (showTotalCount)
                ca.label.text += $" ({resultList.Count})";

            resultList.Sort();
            for (int i = 0; i < resultList.Count; i++)
            {
                ItemCell cell = AddItem(ca, resultList[i]);
                itemCells.Add(cell);
                cell.data = new ItemCell.ModelData(0, label, resultList[i]);
                cell.GetComponentInChildren<ToolTip>().context = cell.label.text;
               
                cell.cellButton.onClick.AddListener(delegate
                {
                    clickedItem = cell;
                    gameObject.SetActive(false);
                    OnClick?.Invoke();
                });
                //Web server에 저장된 이미지를 불러옴 
                ImageRequestManager.Instance.DownloadImage(cell.label.text, (result) =>
                {
                    var tex2D = (Texture2D)result;
                    var rect = new Rect(0, 0, tex2D.width, tex2D.height);
                    cell.thumbnail.sprite = Sprite.Create(tex2D, rect, new Vector2(0.5f, 0.5f));
                });
            }

            categoryDic.Add(ca, itemCells);
            LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
        });
    }


    #region AddItems
    public Category AddCategory(string label)
    {
        Category ca = Instantiate(CategoryPrefab, content).GetComponent<Category>();
        ca.name = label;
        ca.label.text = label;
        return ca;
    }
    public ItemCell AddItem(Category category, string key)
    {
        ItemCell item = Instantiate(CellPrefab, category.container.transform).GetComponent<ItemCell>();
        item.name = key;
        item.label.text = key;
        return item;
    }

    public Category GetCategory(string label)
    {
        Category ca = content.transform.Find(label).GetComponent<Category>();
        return ca;
    }

    public List<ItemCell> GetItems(Category category)
    {
        List<ItemCell> itemList = new List<ItemCell>();
        itemList = category.GetComponentsInChildren<ItemCell>().ToList();
        return itemList;
    }
    #endregion

    #region UGUI Event
    public void OnClick_SearchBtn()
    {
        blockbar.DOFillAmount(1f, 0.4f);
    }
    public void OnDisableSearch()
    {
        blockbar.DOFillAmount(0f, 0.4f);
    }
    public void OnSearching()
    {
        foreach (var item in content.GetComponentsInChildren<ItemCell>(true))
        {
            string label = item.label.text.ToLower();
            string input = searchInput.InputTextToLower();
            if (label.Contains(input))
            {
                item.gameObject.SetActive(true);
            }
            else item.gameObject.SetActive(false);
        }
    }

    public void OnValueChanged_EditToggle(Toggle toggle)
    {
        if (toggle.isOn)
        {
            toggle.GetComponent<ToolTip>().context = "저장";
            blockbar.DOFillAmount(1f, 0.4f);
            toggle.GetComponent<RectTransform>().DOAnchorPosX(editToggleOriginpos.x + 70f, 0.4f);

            foreach (var category in categoryDic.Keys)
            {
                foreach (var item in categoryDic[category])
                {
                    item.cellButton.interactable = false;
                }
                
            }
        }
        else
        {
            toggle.GetComponent<ToolTip>().context = "이미지 수정";
            blockbar.DOFillAmount(0f, 0.4f);
            toggle.GetComponent<RectTransform>().DOAnchorPosX(editToggleOriginpos.x, 0.4f);

            foreach (var category in categoryDic.Keys)
            {
                foreach (var item in categoryDic[category])
                {
                    item.cellButton.interactable = true;

                    if (item.isEdited) //사용자가 수정한 것만 Webserver로 업로드
                    {
                        ImageRequestManager.Instance.UploadImage(item.thumbnail, item.label.text, (result) =>
                        {
                            item.isEdited = false;
                            Debug.Log("썸네일 업로드 완료 : " + result);
                        });
                    }
                }
            }
        }
        for (int i = 0; i < itemCells.Count; i++)
        {
            itemCells[i].AddIMGButton.gameObject.SetActive(toggle.isOn);
        }
    }

    public void Sort(Dropdown dropdown)
    {
        List<ItemCell> newSortedList = new List<ItemCell>();
        if (dropdown.value == 0) //오름차순
        {
            foreach (var category in categoryDic.Keys)
            {
                newSortedList = categoryDic[category].OrderBy(x => x.label.text).ToList();
            }
        }
        else if (dropdown.value == 1) //내림차순
        {
            foreach (var category in categoryDic.Keys)
            {
                newSortedList = categoryDic[category].OrderByDescending(x => x.label.text).ToList();
            }
        }

        foreach (var item in newSortedList)
        {
            item.transform.SetAsLastSibling();
        }
    }

    public void OnClick_RefreshBtn()
    {
        foreach (var category in categoryDic.Keys)
        {
            foreach (var item in categoryDic[category])
            {
                ImageRequestManager.Instance.DownloadImage(item.label.text, (result) =>
                {
                    var tex2D = (Texture2D)result;
                    var rect = new Rect(0, 0, tex2D.width, tex2D.height);
                    item.thumbnail.sprite = Sprite.Create(tex2D, rect, new Vector2(0.5f, 0.5f));
                });
            }

        }
    }
    #endregion
}
