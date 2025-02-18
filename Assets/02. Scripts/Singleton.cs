using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;
    private static readonly object _lock = new object();
    private static bool _instanceDestroyed = false;  // applicationIsQuitting 대신 instanceDestroyed 사용

    public static T Instance
    {
        get
        {
            lock (_lock)
            {
                // 인스턴스가 없거나 파괴된 경우 새로 생성
                if (_instance == null)
                {
                    _instanceDestroyed = false;  // 새 인스턴스 생성 시 플래그 리셋
                    _instance = (T)FindObjectOfType(typeof(T));

                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject();
                        _instance = singletonObject.AddComponent<T>();
                        singletonObject.name = typeof(T).ToString() + " (Singleton)";
                        Debug.Log($"[Singleton] An instance of {typeof(T)} is created.");
                    }
                }
                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instanceDestroyed = true;
            _instance = null;
        }
    }
}