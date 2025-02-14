using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Object = UnityEngine.Object;

/// <summary>
/// Addressable Asset System�� ����Ͽ� ������ �������� �ε��ϰ� �����ϴ� Ŭ����
/// </summary>
public class AddressableAssetLoader : MonoBehaviour
{
    // ���� �� ���� (Inspector���� ����)
    public AssetLabelReference assetLabel;

    // �ε�� ���µ��� ĳ���ϴ� Dictionary
    public static Dictionary<object, Object> loadedDataDic = new Dictionary<object, Object>();

    // Ÿ�Ժ� �񵿱� �ε� �۾��� �����ϴ� Dictionary
    public static Dictionary<Type, AsyncOperationHandle> opHandles = new Dictionary<Type, AsyncOperationHandle>();

    /// <summary>
    /// ������ Ű�� ������ �񵿱������� �ٿ�ε��ϰ� �ε��մϴ�.
    /// </summary>
    /// <typeparam name="T">�ε��� ������ Ÿ��</typeparam>
    /// <param name="key">������ Addressable Ű</param>
    /// <param name="callback">�ε� �Ϸ� �� ����� �ݹ�</param>
    public static void Download<T>(string key, Action<T> callback = null) where T : Object
    {
        // �̹� �ε�� �����̸� �ߺ� �ε� ����
        if (loadedDataDic.ContainsKey(key))
            return;

        // �񵿱� �ε� �۾� ����
        var opHandle = Addressables.LoadAssetAsync<T>(key);
        opHandles[typeof(T)] = opHandle;

        opHandle.Completed += operation =>
        {
            T loadedAsset;
            // GameObject�� ��� �ν��Ͻ� ����
            if (typeof(T) == typeof(GameObject))
            {
                loadedAsset = Instantiate(opHandle.Result as T);
            }
            else
            {
                loadedAsset = opHandle.Result as T;
            }

            // �ε�� ���� ĳ�� �� �ݹ� ����
            loadedDataDic.Add(key, loadedAsset);
            callback?.Invoke(loadedAsset);
        };
    }

    /// <summary>
    /// �ε�� ������ �޸𸮿��� �����մϴ�.
    /// </summary>
    /// <typeparam name="T">������ ������ Ÿ��</typeparam>
    /// <param name="key">������ Ű</param>
    public static void UnLoadData<T>(object key) where T : Object
    {
        // �񵿱� �ε� �۾� �ڵ� ����
        if (opHandles.TryGetValue(typeof(T), out AsyncOperationHandle handle) && handle.IsValid())
        {
            Addressables.Release(handle);
            opHandles.Remove(typeof(T));
        }

        // �ε�� ���� ����
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
    /// Ư�� ���� ���� ��� Addressable ������ Ű�� �񵿱������� �����ɴϴ�.
    /// </summary>
    /// <param name="label">�˻��� ��</param>
    /// <param name="onComplete">Ű ����� ��ȯ���� �ݹ�</param>
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