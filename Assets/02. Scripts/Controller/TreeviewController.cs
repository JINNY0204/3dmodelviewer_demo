using System.Collections.Generic;
using UnityEngine;
using Battlehub.UIControls;
using System;
using System.Linq;
using UnityEngine.UI;
using DG.Tweening;
using System.Data;

public class TreeviewController : MonoBehaviour
{
    IEnumerable<GameObject> dataItems;
    public bool foldOnAwake;
    public TreeView treeView;
    public static TreeView TreeView;
    public bool CanDrag;
    public bool CanEdit;
    public bool CanDrop;

    public Transform panel;
    static Transform Panel;
    public Image bar;
    public Button foldBtn;
    public SearchInputField searchInput;
    public Toggle editToggle;

    static TreeViewItem selectedItem;

    private void Awake()
    {
        if (foldOnAwake)
            bar.fillAmount = 0f;
        else
            bar.fillAmount = 1f;
        panel.gameObject.SetActive(!foldOnAwake);
        Panel = panel;
        TreeView = treeView;
    }


    void SetDataTarget(Transform target)
    {
        var arr = target.GetComponentsInChildren<Transform>(true);
        var resultArray = Array.ConvertAll(arr, e => e.gameObject).Where(go => go.transform.parent == null).OrderBy(t => t.transform.GetSiblingIndex());
        dataItems = resultArray;
        TreeView.Items = dataItems;
        //TreeView.Expand(panel.GetComponentInChildren<TreeViewItem>());//최상위 Item만 Expand

        Debug.Log(target + "의 계층뷰 생성", target.gameObject);
    }

    public static void SelectBindingObject(GameObject target)
    {
        if (TreeView.GetTreeViewItem(target) == selectedItem) return;

        // target에 해당하는 TreeviewItem 찾아 Select 처리
        if (TreeView.GetTreeViewItem(target))
        {
            OnSelectGUI(TreeView.GetTreeViewItem(target));
            return;
        }

        // 없으면, 모델 구조로부터 target의 계층 경로 찾기
        Transform parentTransform = target.transform.parent;
        string hierarchyPath = target.name;

        while (parentTransform != null)
        {
            hierarchyPath = parentTransform.name + "/" + hierarchyPath;
            parentTransform = parentTransform.parent;
        }
        Debug.Log("Hierarchy Path: " + hierarchyPath);

        //경로와 이름을 비교하여 차례로 Expand 하기
        string[] itemArray = hierarchyPath.Split('/');
        for (int i = 0; i < itemArray.Length; i++)
        {
            foreach (var item in Panel.GetComponentsInChildren<TreeViewItem>(true))
            {
                if (item.ItemPresenter.GetComponentInChildren<Text>(true).text.Equals(itemArray[i]))
                {
                    item.CanExpand = true;
                    item.IsExpanded = true;

                    //최종적으로 선택한 모델에 대한 item 선택
                    if (i == itemArray.Length - 1)
                        OnSelectGUI(item);
                }
            }
        }
    }

    static void OnSelectGUI(TreeViewItem targetItem)
    {
        if (selectedItem)
            selectedItem.transform.Find("Background/FixedSelection").gameObject.SetActive(false);

        targetItem.transform.Find("Background/FixedSelection").gameObject.SetActive(true);
        selectedItem = targetItem;

        ScrollToSelection(targetItem);
    }

    #region Event Subscribe/UnSubscribe
    private void Start()
    {
        ProcessManager.Instance.OnLoadedHandler += OnInitialized;
        ProcessManager.Instance.OnReleasedHandler += OnReleased;

        TreeView.ItemDataBinding += OnItemDataBinding;
        TreeView.SelectionChanged += OnSelectionChanged;
        TreeView.ItemDoubleClick += OnItemDoubleClicked;
        TreeView.ItemExpanding += OnItemExpanding;
        TreeView.ItemBeginDrag += OnItemBeginDrag;
        TreeView.ItemBeginDrop += OnItemBeginDrop;
        TreeView.ItemEndDrag += OnItemEndDrag;   
        TreeView.ItemDrop += OnItemDrop;
        TreeView.ItemsRemoved += OnItemsRemoved;
    }

    private void OnDestroy()
    {
        if (ProcessManager.Instance)
        {
            ProcessManager.Instance.OnLoadedHandler -= OnInitialized;
            ProcessManager.Instance.OnReleasedHandler -= OnReleased;
        }
        TreeView.ItemDataBinding -= OnItemDataBinding;
        TreeView.SelectionChanged -= OnSelectionChanged;
        TreeView.ItemDoubleClick -= OnItemDoubleClicked;
        TreeView.ItemExpanding -= OnItemExpanding;
        TreeView.ItemBeginDrag -= OnItemBeginDrag;
        TreeView.ItemEndDrag -= OnItemEndDrag;
        TreeView.ItemBeginDrop -= OnItemBeginDrop;
        TreeView.ItemDrop -= OnItemDrop;
        TreeView.ItemsRemoved -= OnItemsRemoved;
    }
    private void OnInitialized(object sender, LoadDataEventArgs e)
    {
        if (e.item != null)
        {
            SetDataTarget(e.item.transform);
        }
    }
    private void OnReleased(object sender, Transform targetModel)
    {
        if (panel.gameObject.activeSelf)
        {
            searchInput.DisableSearchMode();
            foldBtn.GetComponentInChildren<TMPro.TMP_Text>().DOFade(0f, 0.2f);
            bar.DOFillAmount(0f, 0.4f);
            Panel.gameObject.SetActive(false);
        }
        foreach (var item in Panel.GetComponentsInChildren<TreeViewItem>(true))
        { 
            DestroyImmediate(item.gameObject);
        }
    }
    #endregion

    #region Event Handler
    private void OnItemDataBinding(object sender, TreeViewItemDataBindingArgs e)
    {
        GameObject dataItem = e.Item as GameObject;

        if (dataItem != null)
        {
            TreeViewItem tv = e.ItemPresenter.GetComponent<TreeViewItem>();
            tv.CanDrag = CanDrag;
            tv.CanEdit = CanEdit;
            tv.CanDrop = CanDrop;

            if (tv == selectedItem)
                tv.transform.Find("Background/FixedSelection").gameObject.SetActive(true);

            SetActiveToggleEvent(tv, dataItem);

            Text text = e.ItemPresenter.GetComponentInChildren<Text>(true);
            text.text = dataItem.name;

            Image icon = e.ItemPresenter.GetComponentsInChildren<Image>()[4];

            if (dataItem == ProcessManager.Instance.loadedModel.gameObject)
                icon.sprite = Resources.Load<Sprite>("Ancestor");
            else
            {
                if (dataItem.GetComponent<Renderer>() && dataItem.GetComponent<Renderer>().enabled)
                    icon.sprite = Resources.Load<Sprite>("mesh");
                else icon.sprite = Resources.Load<Sprite>("empty");
            }
                
            if (dataItem.name != "TreeView")
                e.HasChildren = dataItem.transform.childCount > 0;
        }
    }
    
    private void OnItemDoubleClicked(object sender, ItemArgs e)
    {
        //더블클릭한 TreeviewItem에 바인딩되어 있는 데이터 (게임오브젝트)에 대한 처리
        GameObject target = (GameObject)e.Items[0];
        if (target == null || target == ModelModifier.Instance.SelectedModel.gameObject || target == ProcessManager.Instance.loadedModel.gameObject) return;

        if (target.GetComponentsInChildren<ModelClicker>().Length > 0)
        {
            if (target && ModelModifier.Instance.SelectedModel)
            {
                foreach (var item in ModelModifier.Instance.SelectedModel.GetComponentsInChildren<ModelClicker>())
                {
                    item.OnReleased();
                    ModelModifier.Instance.meshHighlighter.HighlightOff(item.transform);
                }
            }

            foreach (var item in target.GetComponentsInChildren<ModelClicker>())
            {
                ModelModifier.Instance.meshHighlighter.Highlight(0f, item.transform);

                if (item.GetComponent<Outline>())
                {
                    item.GetComponent<Outline>().enabled = false;
                    item.GetComponent<Outline>().OutlineColor = ModelModifier.Instance.selectOutline;
                    item.GetComponent<Outline>().enabled = true;
                }
            }
        }
        else
        {
            ProcessManager.Instance.UserSystemMessage("선택한 항목의 mesh 정보가 없습니다.");
            TreeView.GetTreeViewItem(e.Items[0]).IsSelected = false;
            return;
        }

        //더블클릭한 TreeviewItem UGUI로 표시
        OnSelectGUI(TreeView.GetTreeViewItem(e.Items[0]));
        ModelModifier.Instance.SelectedModel = target.transform;
        UILayoutManager.Instance.detailInformation.UpdateData(target.transform, false);
        ModelModifier.Instance.meshHighlighter.Highlight(0F, target.transform);
        ViewModeController.Instance.ChangeView(ViewModeController.Direction.Quater, target.transform);

#if UNITY_EDITOR
        UnityEditor.Selection.objects = e.Items.OfType<GameObject>().ToArray();
#endif
    }

    private void OnSelectionChanged(object sender, SelectionChangedArgs e)
    {

    }

    private void OnItemBeginDrop(object sender, ItemDropCancelArgs e)
    {
        
    }
    private void OnItemEndDrag(object sender, ItemArgs e)
    {
        
    }

    private void OnItemBeginDrag(object sender, ItemArgs e)
    {
       
    }
    private void OnItemDrop(object sender, ItemDropArgs e)
    {
        if (e.DropTarget == null)
        {
            return;
        }

        Transform dropT = ((GameObject)e.DropTarget).transform;

        //Set drag items as children of drop target
        if (e.Action == ItemDropAction.SetLastChild)
        {
            for (int i = 0; i < e.DragItems.Length; ++i)
            {
                Transform dragT = ((GameObject)e.DragItems[i]).transform;
                dragT.SetParent(dropT, true);
                dragT.SetAsLastSibling();
            }
        }

        //Put drag items next to drop target
        else if (e.Action == ItemDropAction.SetNextSibling)
        {
            for (int i = e.DragItems.Length - 1; i >= 0; --i)
            {
                Transform dragT = ((GameObject)e.DragItems[i]).transform;
                int dropTIndex = dropT.GetSiblingIndex();
                if (dragT.parent != dropT.parent)
                {
                    dragT.SetParent(dropT.parent, true);
                    dragT.SetSiblingIndex(dropTIndex + 1);
                }
                else
                {
                    int dragTIndex = dragT.GetSiblingIndex();
                    if (dropTIndex < dragTIndex)
                    {
                        dragT.SetSiblingIndex(dropTIndex + 1);
                    }
                    else
                    {
                        dragT.SetSiblingIndex(dropTIndex);
                    }
                }
            }
        }

        //Put drag items before drop target
        else if (e.Action == ItemDropAction.SetPrevSibling)
        {
            for (int i = 0; i < e.DragItems.Length; ++i)
            {
                Transform dragT = ((GameObject)e.DragItems[i]).transform;
                if (dragT.parent != dropT.parent)
                {
                    dragT.SetParent(dropT.parent, true);
                }

                int dropTIndex = dropT.GetSiblingIndex();
                int dragTIndex = dragT.GetSiblingIndex();
                if (dropTIndex > dragTIndex)
                {
                    dragT.SetSiblingIndex(dropTIndex - 1);
                }
                else
                {
                    dragT.SetSiblingIndex(dropTIndex);
                }
            }
        }
    }

    private void OnItemExpanding(object sender, ItemExpandingArgs e)
    {
        GameObject gameObject = (GameObject)e.Item;
        if (gameObject.transform.childCount > 0)
        {
            GameObject[] children = new GameObject[gameObject.transform.childCount];
            for (int i = 0; i < children.Length; ++i)
            {
                children[i] = gameObject.transform.GetChild(i).gameObject;
            }

            e.Children = children;
        }
    }
    private void OnItemsRemoved(object sender, ItemsRemovedArgs e)
    {
        for (int i = 0; i < e.Items.Length; ++i)
        {
            GameObject go = (GameObject)e.Items[i];
            if (go != null)
            {
                Destroy(go);
            }
        }
    }
    #endregion

    #region UGUIEvent
    public void OnClick_FoldButton()
    {
        panel.gameObject.SetActive(!panel.gameObject.activeSelf);

        if (!panel.gameObject.activeSelf)
        {
            searchInput.DisableSearchMode();
            foldBtn.GetComponentInChildren<TMPro.TMP_Text>().DOFade(0f, 0.2f);
            bar.DOFillAmount(0f, 0.4f);
        }
        else
        {
            if (selectedItem)
                OnSelectGUI(selectedItem);

            foldBtn.GetComponentInChildren<TMPro.TMP_Text>().DOFade(1f, 0.2f);
            bar.DOFillAmount(1f, 0.4f);
        }
    }

    public void OnSearching()
    {
        foreach (var item in Panel.GetComponentsInChildren<TreeViewItem>(true))
        {
            Debug.Log(item.name, item.gameObject);
            string label = item.GetComponentInChildren<Text>(true).text.ToLower();
            string input = searchInput.InputTextToLower();
            if (label.Contains(input))
            {
                item.gameObject.SetActive(true);
            }
            else item.gameObject.SetActive(false);
        }
    }

    // 선택된 Item으로 포커싱하기
    public static void ScrollToSelection(TreeViewItem target)
    {
        Canvas.ForceUpdateCanvases();
        ScrollRect scrollRect = target.GetComponentInParent<ScrollRect>();
        Transform contentPanel = scrollRect.content;

        Vector2 targetAnchorPos =
            (Vector2)scrollRect.transform.InverseTransformPoint(contentPanel.position)
          - (Vector2)scrollRect.transform.InverseTransformPoint(target.transform.position);

        float paddingY = scrollRect.GetComponent<RectTransform>().rect.height / 2f;
        targetAnchorPos.y -= paddingY;

        scrollRect.content.DOAnchorPos(targetAnchorPos, 0.4f);
    }

    public void ToggleEditMode(Toggle toggle)
    {
        foreach (var item in Panel.GetComponentsInChildren<TreeViewItem>(true))
        {
            item.transform.Find("Background/ActiveToggle").gameObject.SetActive(toggle.isOn);
        }
    }

    private void SetActiveToggleEvent(TreeViewItem tv, GameObject dataItem)
    {
        Toggle activeToggle = tv.transform.Find("Background/ActiveToggle").GetComponent<Toggle>();
        activeToggle.isOn = dataItem.activeSelf;

        activeToggle.gameObject.SetActive(editToggle.isOn);
        activeToggle.onValueChanged.AddListener((value) => {
            foreach (var child in dataItem.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.SetActive(value);
                TreeViewItem child_tv = TreeView.GetTreeViewItem(child.gameObject);
                if (child_tv)
                    child_tv.transform.Find("Background/ActiveToggle").GetComponent<Toggle>().isOn = value;
            }
        });
    }
    #endregion
}
