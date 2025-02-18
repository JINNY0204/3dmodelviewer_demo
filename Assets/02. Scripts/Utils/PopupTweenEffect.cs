using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PopupTweenEffect : MonoBehaviour
{
    Vector3 originalScale;
    public Function onEnableComplete;
    //public Function onDisableComplete;
    public Ease tweenEase = Ease.InOutBack;
    public enum Axis { X, Y, Z}
    public Axis axis = Axis.X;
    public float duration = 0.3f;

    void Awake()
    {
        originalScale = GetComponent<RectTransform>().localScale;
    }

    public void OnEnable()
    {
        Open();
    }

    public void Open()
    {
        gameObject.SetActive(true);
        transform.DOKill();

        switch (axis)
        {
            case Axis.X:
                GetComponent<RectTransform>().localScale = new Vector3(0, originalScale.y, originalScale.z);
                GetComponent<RectTransform>().DOScaleX(originalScale.x, duration).SetEase(tweenEase).OnComplete(delegate { transform.DOKill(); onEnableComplete?.Invoke(); });
                break;
            case Axis.Y:
                GetComponent<RectTransform>().localScale = new Vector3(originalScale.x, 0, originalScale.z);
                GetComponent<RectTransform>().DOScaleY(originalScale.y, duration).SetEase(tweenEase).OnComplete(delegate { transform.DOKill(); onEnableComplete?.Invoke(); });
                break;
            case Axis.Z:
                GetComponent<RectTransform>().localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
                GetComponent<RectTransform>().DOScaleZ(originalScale.z, duration).SetEase(tweenEase).OnComplete(delegate { transform.DOKill(); onEnableComplete?.Invoke(); });
                break;
        }
    }

    public void Close(Function onDisableComplete = null)
    {
        transform.DOKill();
        switch (axis)
        {
            case Axis.X:
                GetComponent<RectTransform>().localScale = originalScale;
                GetComponent<RectTransform>().DOScaleX(0f, duration).SetEase(tweenEase).OnComplete(delegate { transform.DOKill(); onDisableComplete?.Invoke(); });
                break;
            case Axis.Y:
                GetComponent<RectTransform>().localScale = originalScale;
                GetComponent<RectTransform>().DOScaleY(0f, duration).SetEase(tweenEase).OnComplete(delegate { transform.DOKill(); onDisableComplete?.Invoke(); });
                break;
            case Axis.Z:
                GetComponent<RectTransform>().localScale = originalScale;
                GetComponent<RectTransform>().DOScaleZ(0f, duration).SetEase(tweenEase).OnComplete(delegate { transform.DOKill(); onDisableComplete?.Invoke(); });
                break;
        }
    }

    void OnDisable()
    {
        transform.DOKill();
        GetComponent<RectTransform>().localScale = originalScale;
        //switch (axis)
        //{
        //    case Axis.X:
        //        GetComponent<RectTransform>().localScale = originalScale;
        //        GetComponent<RectTransform>().DOScaleX(0f, duration).SetEase(tweenEase).OnComplete(delegate { transform.DOKill(); onDisableComplete?.Invoke(); });
        //        break;
        //    case Axis.Y:
        //        GetComponent<RectTransform>().localScale = originalScale;
        //        GetComponent<RectTransform>().DOScaleY(0f, duration).SetEase(tweenEase).OnComplete(delegate { transform.DOKill(); onDisableComplete?.Invoke(); });
        //        break;
        //    case Axis.Z:
        //        GetComponent<RectTransform>().localScale = originalScale;
        //        GetComponent<RectTransform>().DOScaleZ(0f, duration).SetEase(tweenEase).OnComplete(delegate { transform.DOKill(); onDisableComplete?.Invoke(); });
        //        break;
        //}
    }
}
