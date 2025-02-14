using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Disassembler : MonoBehaviour
{
    public class SubMeshes
    {
        public MeshRenderer meshRenderer;
        public Vector3 originalPosition;
        public Vector3 explodedPosition;
    }

    #region properties
    public static Disassembler Instance;
    enum Mode { Assemble, Disassemble, Explode }
    Mode mode = Mode.Assemble;

    public List<SubMeshes> childMeshRenderers;
    public float duration = 2f;
    public float delay = 2f;
    public float explosionDistance = 1.5f;

    [Header("Buttons")]
    public Button assembleBtn;
    public Button disassembleBtn;
    public Button explodeBtn;
    Button prevSelectedBtn;
    #endregion

    #region private
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    private void OnDestroy()
    {
        if (ProcessManager.Instance)
        {
            ProcessManager.Instance.OnLoadedHandler -= OnInitialized;
            ProcessManager.Instance.OnReleasedHandler -= OnReleased;
        }
    }
    private void OnInitialized(object sender, LoadDataEventArgs e)
    {
        Debug.Log("Disassembler에서 이벤트핸들러 실행");
        OnClick_Btn(Mode.Assemble);
        SetData(e.item.transform);
    }
    private void OnReleased(object sender, Transform targetModel)
    {
        OnClick_Btn(Mode.Assemble);
        StopAllCoroutines();
    }

    private void Start()
    {
        ProcessManager.Instance.OnLoadedHandler += OnInitialized;
        ProcessManager.Instance.OnReleasedHandler += OnReleased;
        DOTween.SetTweensCapacity(10000, 1000);

        assembleBtn.onClick.AddListener(delegate 
        {
            if (ProcessManager.Instance.loadedModel == null) return;
            if (mode != Mode.Assemble)
            {
                OnClick_Btn(Mode.Assemble);
                StopAllCoroutines();
                PlayCor(Mode.Assemble);
            }
        });
        disassembleBtn.onClick.AddListener(delegate 
        {
            if (ProcessManager.Instance.loadedModel == null) return;
            if (mode != Mode.Disassemble)
            {
                OnClick_Btn(Mode.Disassemble);
                StopAllCoroutines();
                PlayCor(Mode.Disassemble);
            }
        });
        explodeBtn.onClick.AddListener(delegate 
        {
            if (ProcessManager.Instance.loadedModel == null) return;
            if (mode != Mode.Explode)
            {
                OnClick_Btn(Mode.Explode);
                StopAllCoroutines();
                PlayCor(Mode.Explode);
            }
        });

        OnClick_Btn(Mode.Assemble);
        //Assemble();
    }
    private void SetData(Transform originalEquipModel)
    {
        childMeshRenderers = new List<SubMeshes>();
        foreach (var item in originalEquipModel.GetComponentsInChildren<MeshRenderer>())
        {
            SubMeshes mesh = new SubMeshes();
            mesh.meshRenderer = item;
            mesh.originalPosition = item.transform.position;
            mesh.explodedPosition = item.bounds.center * explosionDistance;
            childMeshRenderers.Add(mesh);
        }
        assembleBtn.interactable = true;
        disassembleBtn.interactable = true;
        explodeBtn.interactable = true;
    }

    void PlayCor(Mode mode)
    {
        if (childMeshRenderers == null) return;

        this.mode = mode;
        float delayTime = 0f;

        if (mode == Mode.Assemble)
        {
            foreach (var item in childMeshRenderers)
            {
                delayTime += delay;
                item.meshRenderer.transform.DOKill();
                item.meshRenderer.transform.DOMove(item.originalPosition, duration).SetDelay(delayTime);
            }
        }
        else if (mode == Mode.Disassemble)
        {
            foreach (var item in childMeshRenderers)
            {
                delayTime += delay;
                item.meshRenderer.transform.DOKill();
                if (item.meshRenderer.transform == ModelModifier.Instance.SelectedModel)
                    item.meshRenderer.transform.DOMove(item.explodedPosition, duration).SetDelay(delayTime);
                else
                    item.meshRenderer.transform.DOMove(item.originalPosition, duration).SetDelay(delayTime);
            }
        }
        else if (mode == Mode.Explode)
        {
            foreach (var item in childMeshRenderers)
            {
                delayTime += delay;
                item.meshRenderer.transform.DOKill();
                item.meshRenderer.transform.DOMove(item.explodedPosition, duration).SetDelay(delayTime);
            }
        }
    }

    void OnClick_Btn(Mode mode)
    {
        Button button = null;
        if (mode == Mode.Assemble)
            button = assembleBtn;
        else if (mode == Mode.Disassemble)
            button = disassembleBtn;
        else if (mode == Mode.Explode)
            button = explodeBtn;

        if (prevSelectedBtn)
            prevSelectedBtn.GetComponent<Image>().color = prevSelectedBtn.colors.normalColor;

        button.GetComponent<Image>().color = button.colors.selectedColor;
        prevSelectedBtn = button;
    }
    #endregion

    public void Initialize()
    {
        //Assemble();
        assembleBtn.onClick.Invoke();
    }
}