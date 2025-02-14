using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Object = UnityEngine.Object;

/// <summary>
/// Addressable Asset System을 사용하여 에셋을 동적으로 로드하고 관리하는 클래스
/// </summary>
public class AddressableAssetLoader : MonoBehaviour
{
    // 에셋 라벨 참조 (Inspector에서 설정)
    public AssetLabelReference assetLabel;

    // 로드된 에셋들을 캐싱하는 Dictionary
    public static Dictionary<object, Object> loadedDataDic = new Dictionary<object, Object>();

    // 타입별 비동기 로드 작업을 추적하는 Dictionary
    public static Dictionary<Type, AsyncOperationHandle> opHandles = new Dictionary<Type, AsyncOperationHandle>();

    /// <summary>
    /// 지정된 키의 에셋을 비동기적으로 다운로드하고 로드합니다.
    /// </summary>
    /// <typeparam name="T">로드할 에셋의 타입</typeparam>
    /// <param name="key">에셋의 Addressable 키</param>
    /// <param name="callback">로드 완료 시 실행될 콜백</param>
    public static void Download<T>(string key, Action<T> callback = null) where T : Object
    {
        // 이미 로드된 에셋이면 중복 로드 방지
        if (loadedDataDic.ContainsKey(key))
            return;

        // 비동기 로드 작업 시작
        var opHandle = Addressables.LoadAssetAsync<T>(key);
        opHandles[typeof(T)] = opHandle;

        opHandle.Completed += operation =>
        {
            T loadedAsset;
            // GameObject인 경우 인스턴스 생성
            if (typeof(T) == typeof(GameObject))
            {
                loadedAsset = Instantiate(opHandle.Result as T);
            }
            else
            {
                loadedAsset = opHandle.Result as T;
            }

            // 로드된 에셋 캐싱 및 콜백 실행
            loadedDataDic.Add(key, loadedAsset);
            callback?.Invoke(loadedAsset);
        };
    }

    /// <summary>
    /// 로드된 에셋을 메모리에서 해제합니다.
    /// </summary>
    /// <typeparam name="T">해제할 에셋의 타입</typeparam>
    /// <param name="key">에셋의 키</param>
    public static void UnLoadData<T>(object key) where T : Object
    {
        // 비동기 로드 작업 핸들 해제
        if (opHandles.TryGetValue(typeof(T), out AsyncOperationHandle handle) && handle.IsValid())
        {
            Addressables.Release(handle);
            opHandles.Remove(typeof(T));
        }

        // 로드된 에셋 해제
        if (loadedDataDic.ContainsKey(key))
        {
            if (loadedDataDic[key] is GameObject)
            {
                Addressables.ReleaseInstance(loadedDataDic[key] as GameObject);
                DestroyImmediate(loadedDataDic[key]);
            }
            else
            {
                Addressables.Release(loadedDataDic[key]);
            }
            loadedDataDic.Remove(key);
        }
    }

    /// <summary>
    /// 특정 라벨을 가진 모든 Addressable 에셋의 키를 비동기적으로 가져옵니다.
    /// </summary>
    /// <param name="label">검색할 라벨</param>
    /// <param name="onComplete">키 목록을 반환받을 콜백</param>
    public static void GetAllAddressableKeys(string label, Action<List<string>> onComplete)
    {
        AsyncOperationHandle<IList<IResourceLocation>> handle = Addressables.LoadResourceLocationsAsync(label);
        handle.Completed += operation =>
        {
            if (operation.Status == AsyncOperationStatus.Succeeded)
            {
                List<string> keys = new List<string>();
                foreach (var location in handle.Result)
                {
                    string key = location.PrimaryKey;
                    if (!keys.Contains(key))
                        keys.Add(key);
                }
                onComplete?.Invoke(keys);
            }
            else
            {
                Debug.Log("Failed to load addressable location : " + operation.DebugName);
            }
            Addressables.Release(handle);
        };
    }
}