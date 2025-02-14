using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ModelSettingEditor : Editor
{
    [MenuItem("My Menu/선택한 모델 세팅")]
    static void SetTargetModel()
    {
        try
        {
            Transform selectModel = Selection.activeGameObject.transform;
            foreach (var item in selectModel.GetComponentsInChildren<Renderer>())
            {
                if (item.enabled == false)
                {
                    Debug.Log(item.name, item.gameObject);
                    continue;
                }

                if (item.GetComponent<Collider>() == null)
                {
                    item.gameObject.AddComponent<MeshCollider>();
                }
                else
                {
                    item.GetComponent<MeshCollider>().convex = false;
                }
                if (item.GetComponent<ModelClicker>() == null)
                {
                    item.gameObject.AddComponent<ModelClicker>();
                }
                if (item.GetComponent<Outline>() == null)
                {
                    Outline outline = item.gameObject.AddComponent<Outline>();
                    outline.OutlineWidth = 3;
                    outline.enabled = false;

                }
            }
            EditorUtility.DisplayDialog("Complete message", "모델 세팅 완료\ncount : " + selectModel.GetComponentsInChildren<MeshRenderer>(true).Length, "확인");
        }
        catch (System.Exception ex)
        {
            if (EditorUtility.DisplayDialog("Error message", "오류 : " + ex, "Rollback", "Ignore"))
            {
                ResetTargetModel();
                return;
            }
        }
    }

    [MenuItem("My Menu/선택한 모델 세팅 초기화")]
    static void ResetTargetModel()
    {
        try
        {
            Transform selectModel = Selection.activeGameObject.transform;
            foreach (var item in selectModel.GetComponentsInChildren<Renderer>(true))
            {
                Collider collider = item.GetComponent<Collider>();

                if (collider)
                {
                    foreach (var col in collider.GetComponentsInChildren<Collider>(true))
                        DestroyImmediate(col);

                }
                if (item.GetComponent<ModelClicker>() != null)
                {
                    DestroyImmediate(item.GetComponent<ModelClicker>());
                }
                if (item.GetComponent<Outline>() != null)
                {
                    DestroyImmediate(item.GetComponent<Outline>());
                }
            }
            EditorUtility.DisplayDialog("Complete message", "초기화 완료\ncount : " + selectModel.GetComponentsInChildren<MeshRenderer>(true).Length, "확인");
        }
        catch (System.Exception ex)
        {
            if (EditorUtility.DisplayDialog("Error message : " + ex, "Rollback", "Ignore"))
            {
                SetTargetModel();
                return;
            }
        }
    }

    [MenuItem("My Menu/Add Rigidbody")]
    static void AddRigidbody()
    {
        Transform selectModel = Selection.activeGameObject.transform;
        foreach (var item in selectModel.GetComponentsInChildren<Collider>(true))
        {
            Rigidbody rigid = null;
            if (item.GetComponent<Rigidbody>() == null)
                rigid = item.gameObject.AddComponent<Rigidbody>();
            else
                rigid = item.GetComponent<Rigidbody>();
            rigid.isKinematic = true;
            rigid.useGravity = false;

            //MeshCollider meshCol = item.GetComponent<MeshCollider>();
            //if (meshCol)
            //    meshCol.convex = true;
        }
    }

    [MenuItem("My Menu/Remove Rigidbody")]
    static void RemoveRigidbody()
    {
        Transform selectModel = Selection.activeGameObject.transform;
        foreach (var item in selectModel.GetComponentsInChildren<Rigidbody>(true))
        {
            DestroyImmediate(item.GetComponent<Rigidbody>());
        }
    }

    static List<bool> renderStateList = new List<bool>();
    [MenuItem("My Menu/Save Renderer State")]
    static void GetRendererState()
    {
        renderStateList = new List<bool>();
        Transform selectModel = Selection.activeGameObject.transform;
        foreach (var item in selectModel.GetComponentsInChildren<Renderer>(true))
        {
            renderStateList.Add(item.enabled);
        }
    }
    [MenuItem("My Menu/Set Renderer State from Saved Data")]
    static void SetRendererState()
    {
        int enabledCount = 0;
        if (renderStateList.Count > 0)
        {
            Transform selectModel = Selection.activeGameObject.transform;
            for (int i = 0; i < selectModel.GetComponentsInChildren<Renderer>(true).Length; i++)
            {
                selectModel.GetComponentsInChildren<Renderer>(true)[i].enabled = renderStateList[i];
                if (renderStateList[i])
                    enabledCount++;
            }
            renderStateList = new List<bool>();

            Debug.Log(enabledCount + " / " + renderStateList.Count);
        }
    }
}
