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
    // Addressable에 등록된 FBX 모델의 라벨
    public static string labelName = "Model";
    // 썸네일 저장 경로
    public static string thumbnailSavePath = "Assets/Thumbnails/";
    static LayerMask originLayer;


    [MenuItem("My Menu/썸네일 생성기")]
    static void StartGenerate()
    {
        originLayer = Camera.main.cullingMask;
        // Addressable에서 라벨로 등록된 모든 리소스를 로드
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

            // 각 리소스에 대해 GenerateThumbnail 함수를 별도의 스레드에서 실행
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
        // Addressables에서 리소스 로드
        Addressables.LoadAssetAsync<GameObject>(location).Completed += obj =>
        {
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject modelPrefab = obj.Result;

                // Scene에 모델을 Instantiate
                GameObject modelInstance = Instantiate(modelPrefab);
                // 카메라 위치를 모델에 포커스되게 설정
                ChangeView(modelInstance.transform);
                // 모델을 렌더링하여 썸네일 생성
                Texture2D thumbnail = CaptureThumbnail(modelInstance);

                // 썸네일을 이미지로 저장
                SaveThumbnail(thumbnail, location.PrimaryKey);

                // Scene에 Instantiate된 모델은 더 이상 필요하지 않으므로 제거
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
        // 모델을 렌더링하여 Texture2D로 캡처
        // 예시로 모델을 카메라의 시점에서 렌더링하도록 함
        Camera mainCamera = Camera.main;
        RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
        mainCamera.targetTexture = renderTexture;
        mainCamera.Render();

        Texture2D thumbnail = new Texture2D(1920, 1080);
        RenderTexture.active = renderTexture;
        thumbnail.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
        thumbnail.Apply();

        // 리소스 정리
        RenderTexture.active = null;
        mainCamera.targetTexture = null;

        return thumbnail;
    }

    static void SaveThumbnail(Texture2D thumbnail, string modelName)
    {
        // 썸네일을 이미지로 저장
        byte[] bytes = thumbnail.EncodeToPNG();
        string filePath = thumbnailSavePath + modelName + ".png";
        System.IO.File.WriteAllBytes(filePath, bytes);
        Debug.Log("Thumbnail saved at: " + filePath);
    }

    static void ChangeView(Transform target)
    {
        if (target == null) return;

        //모델 피벗 위치 설정
        Bounds bounds = ModelModifier.RendererBound(target);
        Camera.main.GetComponent<CameraController>().cameraPivot.position = bounds.center;

        //피벗 rotation 및 카메라 distace설정
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

        //값 대입
        Camera.main.transform.DOKill();
        Camera.main.GetComponent<CameraController>().cameraPivot.eulerAngles = modelPivotRotation;
        Camera.main.transform.localEulerAngles = Vector3.zero;
        Camera.main.transform.localPosition = camPos;
    }
}
