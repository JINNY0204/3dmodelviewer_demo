using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

public class MeshHighlighter : MonoBehaviour
{
    public Dictionary<Transform, Coroutine> highlightCorDic = new Dictionary<Transform, Coroutine>();
    public Dictionary<Transform, List<Material>> materialDic = new Dictionary<Transform, List<Material>>();
    public Color emissinoColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);
    //public Material sourceMaterial;
    //Color originalSourceColor;
    //private void Start()
    //{
    //    originalSourceColor = sourceMaterial.color;
    //}
    //private void OnDisable()
    //{
    //    sourceMaterial.SetColor("_Color", originalSourceColor);
    //}
    /// <summary>
    /// 모델의 하이라이트 효과 주기
    /// </summary>
    /// <param name="lifeTime">하이라이트 지속시간. '0'이면 코드에서 수동으로 Off 해주기 전까지 계속 됨</param>
    public void Highlight(float lifeTime, Transform targetModel)
    {
        if (highlightCorDic.ContainsKey(targetModel)) return;

        Coroutine coroutine = StartCoroutine(HighlightOn(targetModel, lifeTime));
        highlightCorDic.Add(targetModel, coroutine);
    }
    public void HighlightOff(Transform targetModel)
    {
        RemoveHighlightCoroutine(targetModel);
        RecoverMaterial(targetModel);
    }
    public void HighlightOff()
    {
        foreach (var target in highlightCorDic.Keys)
        {
            StopCoroutine(highlightCorDic[target]);
            RecoverMaterial(target);
        }
        highlightCorDic.Clear();
    }





    List<Material> matList;
    IEnumerator HighlightOn(Transform target, float lifeTime)
    {
        float timer = 0f;

        #region 방법1. 원래 Material의 EmissionColor만 조정하는 방식
        Renderer[] meshRenderers = target.GetComponentsInChildren<Renderer>();
        matList = new List<Material>();
        for (int j = 0; j < meshRenderers.Length; j++)
        {
            Material[] mats = new Material[meshRenderers[j].materials.Length];
            mats = meshRenderers[j].materials;
            mats = mats.Where(x => !x.name.Contains("Outline")).ToArray();
            matList.AddRange(mats);
        }
        //materialDic.Add(target, matList);

        foreach (var material in matList)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emissinoColor);
        }

        while (true)
        {
            //LifeTime 설정
            if (lifeTime != 0)
            {
                timer += Time.deltaTime;
                if (timer >= lifeTime)
                {
                    HighlightOff(target);
                    yield break;
                }
            }

            foreach (var material in matList)
            {
                material.SetColor("_EmissionColor", Color.Lerp(Color.clear, emissinoColor, Mathf.PingPong(Time.time, 0.5f)));
            }

            yield return null;
        }
        #endregion

        #region 방법2. Material을 아예 다른 것으로 교체 후, 교체 된 Mat의 Color 조정하는 방식
        //sourceMaterial.color = originalSourceColor;

        //Renderer[] _meshRenderers = target.GetComponentsInChildren<Renderer>();
        //for (int j = 0; j < _meshRenderers.Length; j++)
        //{
        //    List<Material> matLists = new List<Material>();
        //    for (int i = 0; i < _meshRenderers[j].materials.Length; i++)
        //        matLists.Add(sourceMaterial);
        //    _meshRenderers[j].materials = matLists.ToArray();
        //}

        //while (true)
        //{
        //    sourceMaterial.SetColor("_Color", Color.Lerp(Color.clear, originalSourceColor, Mathf.PingPong(Time.time, 1f)));
        //    yield return null;
        //}
        #endregion
    }

    /// <summary>
    /// </summary>
    /// 모델의 하이라이트 효과 제거

    void RemoveHighlightCoroutine(Transform key)
    {
        if (highlightCorDic.ContainsKey(key))
        {
            StopCoroutine(highlightCorDic[key]);
            highlightCorDic.Remove(key);
            //materialDic.Remove(key);
        }
    }
    void RecoverMaterial(Transform key)
    {
        //if (materialDic != null && matList != null)
        //{
        //    if (materialDic.ContainsKey(key))
        //    {
        //        foreach (var material in materialDic[key])
        //        {
        //            material.SetColor("_EmissionColor", emissinoColor);
        //            material.DisableKeyword("_EMISSION");
        //        }
        //    }
        //}

        if (matList != null)
            foreach (var material in matList)
            {
                material.SetColor("_EmissionColor", emissinoColor);
                material.DisableKeyword("_EMISSION");
            }
    }
}