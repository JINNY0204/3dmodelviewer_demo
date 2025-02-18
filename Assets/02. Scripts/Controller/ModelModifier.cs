using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ModelModifier : Singleton<ModelModifier>
{
    #region PROPERTIES
    public Color hoverOuline;
    public Color selectOutline;
    public Material coloringMat;
    public Material transparentMat;

    [Header("Toggles & Button")]
    public Toggle useColliderToggle;
    public Toggle highlighterToggle;
    public Toggle coloringToggle;
    public Toggle transparentAllToggle;
    public Toggle HideToggle;
    public Toggle ShowOnlyToggle;
    public Button RepairButton;

    public MeshHighlighter meshHighlighter { get; private set; }
    public List<ModelClicker> modelClickerList { get; private set; }
    Transform selectedModel;
    public Transform SelectedModel
    {
        get { return selectedModel; }
        set
        {
            selectedModel = value;
            if (value != null)
            {
                foreach (var item in ProcessManager.Instance.loadedModel.GetComponentsInChildren<Transform>(true))
                {
                    item.gameObject.layer = LayerMask.NameToLayer("Default");
                }
                foreach (var item in value.GetComponentsInChildren<Transform>(true))
                {
                    item.gameObject.layer = LayerMask.NameToLayer("Selected");
                }
            }
        }
    }
    public Bounds originalModelBounds { get; private set; }
    #endregion

    protected override void Awake()
    {
        base.Awake();
        meshHighlighter = GetComponent<MeshHighlighter>();
    }
    #region EventHandler
    private void Start()
    {
        ProcessManager.Instance.OnLoadedHandler += OnInitialized;
        ProcessManager.Instance.OnReleasedHandler += OnReleased;
    }

    public void OnInitialized(object sender, LoadDataEventArgs e)
    {
        useColliderToggle.interactable = true;
        AddModelComponent(e.item.transform);
        SetData(e.item.transform);
    }
    private void OnReleased(object sender, Transform targetModel)
    {
        Camera.main.cullingMask = CameraController.CullingMask;
        Camera.main.clearFlags = CameraClearFlags.Skybox;

        SelectedModel = null;

        useColliderToggle.interactable = false;
        useColliderToggle.isOn = false;
        highlighterToggle.isOn = false;
        coloringToggle.isOn = false;
        transparentAllToggle.isOn = false;
        HideToggle.isOn = false;
        ShowOnlyToggle.isOn = false;
    }
    #endregion

    public void SetGenerateColliderValue(Toggle toggle)
    {
        if (ProcessManager.Instance.loadedModel == null) return;

        ProcessManager.Instance.generateCollider = toggle.isOn;

        if (toggle.isOn)
        {
            if (ProcessManager.Instance.generateCollider)
                ProcessManager.Instance.loadedModel.GetComponent<ColliderHandler>().AddOrEnableCollider(); //Mesh Collider 추가or활성화
            //AddModelComponent(ProcessManager.Instance.loadedModel);
            ProcessManager.Instance.UserSystemMessage("물리엔진을 활성화합니다", 4f);
        }
        else
        {
            ProcessManager.Instance.loadedModel.GetComponent<ColliderHandler>().DisableCollider(); //Mesh Collider 비활성화
            ProcessManager.Instance.UserSystemMessage("물리엔진을 비활성화합니다.", 4f);

            highlighterToggle.isOn = toggle.isOn;
            coloringToggle.isOn = toggle.isOn;
            HideToggle.isOn = toggle.isOn;
            transparentAllToggle.isOn = toggle.isOn;
            FindObjectOfType<AvatarCreater>().createToggle.interactable = toggle.isOn;
        }

        highlighterToggle.interactable = toggle.isOn;
        coloringToggle.interactable = toggle.isOn;
        HideToggle.interactable = toggle.isOn;
        transparentAllToggle.interactable = toggle.isOn;
        FindObjectOfType<AvatarCreater>().createToggle.interactable = toggle.isOn;


    }

    void AddModelComponent(Transform target)
    {
        try
        {
            target.AddComponent<ColliderHandler>();

            foreach (var item in target.GetComponentsInChildren<Renderer>())
            {
                if (item.enabled == false || item.gameObject.activeSelf == false)
                {
                    continue;
                }

                if (item.GetComponent<ModelClicker>() == null)
                {
                    item.gameObject.AddComponent<ModelClicker>();
                }
                if (item.GetComponent<Outline>() == null)
                {
                    Outline outline = item.gameObject.AddComponent<Outline>();
                    outline.OutlineColor = hoverOuline;
                    outline.OutlineWidth = 3;
                    outline.enabled = false;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log("Error message : " + ex);
        }
    }

    void SetData(Transform Equipment)
    {
        SelectedModel = Equipment;
        meshHighlighter = GetComponent<MeshHighlighter>();

        originalModelBounds = RendererBound(Equipment);

        modelClickerList = Equipment.GetComponentsInChildren<ModelClicker>(true).ToList();
        for (int i = 0; i < modelClickerList.Count; i++)
            modelClickerList[i].Set();
    }

    public bool AnyToggleActive()
    {
        return highlighterToggle.isOn || coloringToggle.isOn || HideToggle.isOn;
    }

    public void OnValueChanged_Highliter(Toggle toggle)
    {
        if (!toggle.isOn) 
        {
            if (SelectedModel)
            {
                foreach (var item in selectedModel.GetComponentsInChildren<ModelClicker>())
                {
                    item.OnReleased();
                    meshHighlighter.HighlightOff(item.transform);
                }
                SelectedModel = ProcessManager.Instance.loadedModel;
            }
            MouseEventManager.Instance.HideTag();
        }
    }

    public void OnValueChanged_TransparentAll(Toggle toggle)
    {
        if (modelClickerList != null)
        {
            if (toggle.isOn)
            {
                foreach (var item in modelClickerList)
                {
                    if (item.transform != SelectedModel)
                        item.Transparent();
                }
            }
            else
            {
                foreach (var item in modelClickerList)
                {
                    if (item.transform != SelectedModel)
                        item.InitState();
                }
            }
        }
    }

    public void OnClick_ResetBtn(bool resetView = true)
    {
        if (modelClickerList != null)
        {
            if (SelectedModel)
                SelectedModel = ProcessManager.Instance.loadedModel;

            foreach (var item in modelClickerList)
                item.InitState();

            if (resetView)
                ViewModeController.Instance.OnClick_Quarter();

            Camera.main.cullingMask = CameraController.CullingMask;
            Camera.main.clearFlags = CameraClearFlags.Skybox;

            highlighterToggle.isOn = false;
            coloringToggle.isOn = false;
            transparentAllToggle.isOn = false;
            HideToggle.isOn = false;
            ShowOnlyToggle.isOn = false;

            Disassembler.Instance.Initialize();
        }
    }

    public void ResetModifyTool()
    {
        highlighterToggle.isOn = false;
        coloringToggle.isOn = false;
        transparentAllToggle.isOn = false;
        HideToggle.isOn = false;
        ShowOnlyToggle.isOn = false;
    }

    public static Bounds RendererBound(Transform target)
    {
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        bool hasBounds = false;

        foreach (Renderer render in target.GetComponentsInChildren<Renderer>())
        {
            if (render is ParticleSystemRenderer) continue;
            if (!render.enabled) continue;

            if (hasBounds)
            {
                bounds.Encapsulate(render.bounds);
            }
            else
            {
                bounds = render.bounds;
                hasBounds = true;
            }
        }
        return bounds;
    }
}