using Unity.VisualScripting;
using UnityEngine;

public class ColliderHandler : MonoBehaviour
{
    Renderer[] renderers;
    Collider[] colliders;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        colliders = new Collider[renderers.Length];
    }

    public void AddOrEnableCollider()
    {
        if (renderers != null)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (colliders[i] == null)
                {
                    Collider col = null;
                    if (renderers[i].enabled)
                    {
                        col = renderers[i].AddComponent<MeshCollider>();
                    }
                    colliders[i] = col;
                }
                else
                {
                    colliders[i].enabled = true;
                }
            }
        }

        //try
        //{
        //    if (meshCol.Length > 0)
        //    {
        //        for (int i = 0; i < meshes.Length; ++i)
        //        {
        //            if (meshCol[i] == null)
        //            {
        //                if (meshFilterArray[i].GetComponent<MeshRenderer>().enabled)
        //                {
        //                    MeshCollider col = meshFilterArray[i].gameObject.AddComponent<MeshCollider>();
        //                    col.sharedMesh = meshes[i];
        //                    meshCol[i] = col;
        //                }
        //            }
        //            else
        //            {
        //                meshCol[i].sharedMesh = meshes[i];
        //                meshCol[i].enabled = true;
        //            }
        //        }
        //    }
        //    else Debug.LogError("BakeMeshJob 컴포넌트를 모델에 추가하세요.");
        //}
        //catch (System.Exception e)
        //{
        //    Debug.Log(e);
        //}
    }

    public void DisableCollider()
    {
        if (renderers != null)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (colliders[i] != null)
                    colliders[i].enabled = false;
            }
        }

        //if (meshCol.Length > 0)
        //{
        //    for (int i = 0; i < meshes.Length; ++i)
        //    {
        //        try
        //        {
        //            if (meshCol[i] != null)
        //                meshCol[i].enabled = false;
        //        }
        //        catch (System.Exception ex)
        //        {
        //            Debug.Log(ex);
        //            ProcessManager.instance.UserSystemMessage("오류 : " + ex);
        //        }
        //    }
        //}
        //else Debug.LogError("BakeMeshJob 컴포넌트를 모델에 추가하세요.");
    }
}
