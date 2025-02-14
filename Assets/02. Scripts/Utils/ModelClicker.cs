using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ModelClicker : MonoBehaviour
{
    Renderer rend;
    Material[] originMatArray;
    Outline outline;
    bool isHovering;
    float doubleClickTimeThreshold = 0.2f;
    float lastClickTime;

    public void Set()
    {
        rend = GetComponent<Renderer>();
        originMatArray = rend.materials;

        if (GetComponent<Outline>())
            outline = GetComponent<Outline>();
    }

    private void OnMouseEnter()
    {
        isHovering = true;
        if (!ModelModifier.Instance.AnyToggleActive()) return;
        if (!MouseEventManager.IsPointerOverUIElement())
        {
            if (outline)
                outline.enabled = true;

            MouseEventManager.Instance.ShowTag(transform.name);
        }
    }

    private void OnMouseExit()
    {
        isHovering = false;
        if (!ModelModifier.Instance.AnyToggleActive()) return;

        if (ModelModifier.Instance.SelectedModel != transform)
            if (outline) 
                outline.enabled = false;

        MouseEventManager.Instance.HideTag();
    }

    private void OnMouseUp()
    {
        if (Time.time - lastClickTime < doubleClickTimeThreshold)
        {
            OnDoubleClicked();
        }
        lastClickTime = Time.time;
    }

    private void OnDoubleClicked()
    {
        if (isHovering && !MouseEventManager.IsPointerOverUIElement())
        {
            if (ModelModifier.Instance.AnyToggleActive())
            {
                if (ModelModifier.Instance.highlighterToggle.isOn)
                {
                    //이전에 선택한것 원복
                    if (ModelModifier.Instance.SelectedModel)
                    {
                        foreach (var item in ModelModifier.Instance.SelectedModel.GetComponentsInChildren<ModelClicker>())
                        {
                            item.OnReleased();
                            ModelModifier.Instance.meshHighlighter.HighlightOff(item.transform);
                        }
                    }
                    OnSelect();
                }
                else if (ModelModifier.Instance.coloringToggle.isOn)
                {
                    if (ModelModifier.Instance.SelectedModel != transform)
                        Coloring();
                    else
                    {
                        ProcessManager.Instance.UserSystemMessage("선택한 모델은 색상을 변경할 수 없습니다.");
                        Debug.Log("하이라이트되지 않은 모델만 색상을 변경할 수 있습니다.");
                    }
                }
                else if (ModelModifier.Instance.HideToggle.isOn)
                {
                    //커맨드 저장
                    ICommand Command = new EraseCommand(gameObject, false);
                    CommandManager.ExecuteCommand(Command);
                }
            }
        }
    }

    public void OnReleased()
    {
        if (ModelModifier.Instance.transparentAllToggle.isOn)
            Transparent();

        if (outline)
        {
            outline.OutlineColor = ModelModifier.Instance.hoverOuline;
            outline.enabled = false;
        }

        UILayoutManager.Instance.detailInformation.ReleaseData(transform);
    }
    public void OnSelect(float highlightTime = 0f)
    {
        if (ModelModifier.Instance.transparentAllToggle.isOn)
            InitState();

        UILayoutManager.Instance.detailInformation.UpdateData(transform, false);

        ModelModifier.Instance.meshHighlighter.Highlight(highlightTime, transform);
        ModelModifier.Instance.SelectedModel = transform;

        TreeviewController.SelectBindingObject(gameObject);

        if (outline)
        {
            outline.enabled = false;
            outline.OutlineColor = ModelModifier.Instance.selectOutline;
            outline.enabled = true;
        }

        if (CameraModeController.Instance.naviMode == CameraModeController.NaviMode.Orbit)
            CameraModeController.Instance.FocusTarget(transform);
    }

    public void Coloring()
    {
        Renderer _rend = GetComponent<Renderer>();
        Material[] _mats = _rend.materials;

        _mats = _mats.Where(x => !x.name.Contains("Outline")).ToArray();

        if (ColorPicker.selectedColor.a != 1)
        {
            for (int i = 0; i < _mats.Length; i++)
                _mats[i] = ModelModifier.Instance.transparentMat;

            _rend.materials = _mats;
        }
        else
        {
            Material colorMat = new Material(ModelModifier.Instance.coloringMat);
            colorMat.color = ColorPicker.selectedColor;

            for (int i = 0; i < _mats.Length; i++)
                _mats[i] = colorMat;

            _rend.materials = _mats;
        }

        //커맨드 저장
        ICommand Command = new ColorChangeCommand(gameObject, originMatArray, _mats);
        CommandManager.ExecuteCommand(Command);
    }
    public void Transparent()
    {
        Renderer _rend = GetComponent<Renderer>();
        Material[] _mats = _rend.materials;
        _mats = _mats.Where(x => !x.name.Contains("Outline")).ToArray();

        for (int i = 0; i < _mats.Length; i++)
            _mats[i] = ModelModifier.Instance.transparentMat;

        _rend.materials = _mats;

        //커맨드 저장
        ICommand Command = new TransparencyChangeCommand(gameObject, originMatArray, _mats);
        CommandManager.ExecuteCommand(Command);
    }
    public void InitState()
    {
        if (originMatArray == null) return;
        if (outline)
        {
            outline.OutlineColor = ModelModifier.Instance.hoverOuline;
            outline.enabled = false;
        }

        Renderer _rend = GetComponent<Renderer>();
        _rend.enabled = true;
        gameObject.SetActive(true);

        Material[] _mats = _rend.materials;
        _mats = _mats.Where(x => !x.name.Contains("Outline")).ToArray();

        for (int i = 0; i < _mats.Length; i++)
        {
            //StandardShaderUtils.ChangeRenderMode(originMatArray[i], StandardShaderUtils.BlendMode.Opaque, 1f);
            _mats[i] = originMatArray[i];
        }
        _rend.materials = _mats;
    }
}