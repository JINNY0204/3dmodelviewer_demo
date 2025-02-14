using DG.Tweening;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class ThumbnailGenerator : Editor
{
    // Addressable�� ��ϵ� FBX ���� ��
    public static string labelName = "Model";
    // ����� ���� ���
    public static string thumbnailSavePath = "Assets/Thumbnails/";
    static LayerMask originLayer;


    [MenuItem("My Menu/����� ������")]
    static void StartGenerate()
    {
        originLayer = Camera.main.cullingMask;
        // Addressable���� �󺧷� ��ϵ� ��� ���ҽ��� �ε�
        Camera.main.cullingMask = 1 << LayerMask.NameToLayer("Default");
        Camera.main.clearFlags = CameraClearFlags.SolidColor;
        Camera.main.backgroundColor = Color.black;

        Addressables.LoadResourceLocationsAsync(labelName, typeof(GameObject)).Completed += OnResourcesLoaded;
    }
    static void OnResourcesLoaded(AsyncOperationHandle<IList<IResourceLocation>> obj)
    {
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            IList<IResourceLocation> locations = obj.Result;

            // �� ���ҽ��� ���� GenerateThumbnail �Լ��� ������ �����忡�� ����
            foreach (var location in locations)
            {
                GenerateThumbnail(location);
            }
        }
        else
        {
            Debug.LogError("Failed to load resource locations with label: " + labelName);
        }
    }
    static void GenerateThumbnail(IResourceLocation location)
    {
        // Addressables���� ���ҽ� �ε�
        Addressables.LoadAssetAsync<GameObject>(location).Completed += obj =>
        {
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject modelPrefab = obj.Result;

                // Scene�� ���� Instantiate
                GameObject modelInstance = Instantiate(modelPrefab);
                // ī�޶� ��ġ�� �𵨿� ��Ŀ���ǰ� ����
                ChangeView(modelInstance.transform);
                // ���� �������Ͽ� ����� ����
                Texture2D thumbnail = CaptureThumbnail(modelInstance);

                // ������� �̹����� ����
                SaveThumbnail(thumbnail, location.PrimaryKey);

                // Scene�� Instantiate�� ���� �� �̻� �ʿ����� �����Ƿ� ����
                DestroyImmediate(modelInstance);
                Camera.main.cullingMask = originLayer;
                Camera.main.clearFlags = CameraClearFlags.Skybox;
            }
            else
            {
                Debug.LogError("Failed to load model at location: " + location.PrimaryKey);
            }
        };
    }
    static Texture2D CaptureThumbnail(GameObject model)
    {
        // ���� �������Ͽ� Texture2D�� ĸó
        // ���÷� ���� ī�޶��� �������� �������ϵ��� ��
        Camera mainCamera = Camera.main;
        RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
        mainCamera.targetTexture = renderTexture;
        mainCamera.Render();

        Texture2D thumbnail = new Texture2D(1920, 1080);
        RenderTexture.active = renderTexture;
        thumbnail.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
        thumbnail.Apply();

        // ���ҽ� ����
        RenderTexture.active = null;
        mainCamera.targetTexture = null;

        return thumbnail;
    }

    static void SaveThumbnail(Texture2D thumbnail, string modelName)
    {
        // ������� �̹����� ����
        byte[] bytes = thumbnail.EncodeToPNG();
        string filePath = thumbnailSavePath + modelName + ".png";
        System.IO.File.WriteAllBytes(filePath, bytes);
        Debug.Log("Thumbnail saved at: " + filePath);
    }

    static void ChangeView(Transform target)
    {
        if (target == null) return;

        //�� �ǹ� ��ġ ����
        Bounds bounds = ModelModifier.RendererBound(target);
        Camera.main.GetComponent<CameraController>().cameraPivot.position = bounds.center;

        //�ǹ� rotation �� ī�޶� distace����
        Vector3 modelPivotRotation = Vector3.zero;
        Vector3 camPos = Vector3.zero;
        float distance = (Camera.main.transform.position - bounds.center).magnitude;
        distance = Mathf.Clamp(distance, (bounds.center - bounds.max).magnitude * 1.5f, (bounds.center - bounds.max).magnitude * 1.5f);

        //switch (direction)
        //{
        //    case Direction.Top:
        //        //distance = Mathf.Clamp(distance, bounds.size.y, bounds.size.y + farDistance);
        //        modelPivotRotation.x = 80f;
        //        camPos.z -= distance;
        //        break;
        //    case Direction.Front:
        //        //distance = Mathf.Clamp(distance, bounds.size.z, bounds.size.z + farDistance);
        //        modelPivotRotation = Vector3.zero;
        //        camPos.z -= distance;
        //        break;
        //    case Direction.Back:
        //        //distance = Mathf.Clamp(distance, bounds.size.z, bounds.size.z + farDistance);
        //        modelPivotRotation.y = 180f;
        //        camPos.z -= distance;
        //        break;
        //    case Direction.Side:
        //        //distance = Mathf.Clamp(distance, bounds.size.x, bounds.size.x + farDistance);
        //        modelPivotRotation.y = 90f;
        //        camPos.z -= distance;
        //        break;
        //    case Direction.Quater:
        //        //distance = Mathf.Clamp(distance, (bounds.center - bounds.max).magnitude, (bounds.center - bounds.max).magnitude);
                 modelPivotRotation.x = 30f;
                 modelPivotRotation.y = 30f;
                 camPos.z -= distance;
        //        break;
        //    default:
        //        break;
        //}

        //�� ����
        Camera.main.transform.DOKill();
        Camera.main.GetComponent<CameraController>().cameraPivot.eulerAngles = modelPivotRotation;
        Camera.main.transform.localEulerAngles = Vector3.zero;
        Camera.main.transform.localPosition = camPos;
    }
}
