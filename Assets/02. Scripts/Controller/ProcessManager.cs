using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;

public delegate void Function();
public delegate void LoadDataEventHandler(object sender, LoadDataEventArgs result);
public delegate void ReleaseDataEventHandler(object sender, Transform targetModel);

public class LoadDataEventArgs : EventArgs
{
    public string key;
    public GameObject item;
}

public class ProcessManager : Singleton<ProcessManager>
{
    public event LoadDataEventHandler OnLoadedHandler;
    public event ReleaseDataEventHandler OnReleasedHandler;

    public AddressableAssetLoader addressableLoader;
    [Header("Model Loaded")]
    public bool generateCollider;
    public Transform Parent;
    public Transform loadedModel { get; private set; }

    [Header("Loading")]
    public float timeout = 120f;
    public GameObject loadingCanvas;
    public Image slider;
    float spanTime;

    [Header("System Message")]
    public Transform systemMessage;
    public Image levelIcon;
    public Sprite normalSprite;
    public Sprite warningSprite;
    public Sprite errorSprite;
    public Color normalColor;
    public Color warningColor;
    public Color errorColor;
    public TMP_Text modelName;

    [Header("assetList")]
    public ModelLibrary modelLibrary;
    string currentKey;

    private void Start()
    {
        modelLibrary.Create(addressableLoader.assetLabel.labelString, delegate
        {
            LoadAsset(ModelLibrary.GetClickedItem().label.text);
        });
    }

    public void Refresh()
    {
        LoadAsset(currentKey);
    }

    void LoadAsset(string newKey)
    {
        if (!string.IsNullOrEmpty(newKey))
        {
            if (!string.IsNullOrEmpty(currentKey))
            {
                UnLoadData(currentKey);
            }
            LoadData(newKey);
        }
    }

    IEnumerator LoadingCor(string key)
    {
        var message = loadingCanvas.GetComponentInChildren<TMPro.TMP_Text>();
        loadingCanvas.SetActive(true);

        yield return new WaitUntil(() => AddressableAssetLoader.opHandles.ContainsKey(typeof(GameObject)));
        var opHandle = AddressableAssetLoader.opHandles[typeof(GameObject)];

        while (!opHandle.IsDone)
        {
            spanTime += Time.deltaTime;
            if (spanTime >= timeout)
            {
                loadingCanvas.SetActive(false);
                AddressableAssetLoader.UnLoadData<GameObject>(key);
                spanTime = 0f;
                yield break;
            }

            var progress = opHandle.PercentComplete;
            slider.fillAmount = progress;

            float percentage = Mathf.Round(progress * 1000) / 10f;
            message.text = $"데이터 다운로드 중...{percentage}%";

            yield return null;
        }

        message.text = "데이터 로드 완료";
        loadingCanvas.SetActive(false);
    }

    void LoadData(string key)
    {
        DOTween.Clear();
        HideMessage();

        StartCoroutine(LoadingCor(key));
        AddressableAssetLoader.Download<GameObject>(key, (resultData) =>
        {
            if (resultData != null)
            {
                Debug.Log($"{key} Loaded");
                UserSystemMessage($"{key}가 다운로드되었습니다.", 3f);
                resultData.name = key; //(Clone)제거
                LoadDataEventArgs eventArgs = new LoadDataEventArgs();
                eventArgs.key = key;
                eventArgs.item = resultData;
                loadedModel = resultData.transform;

                //temp : legacy모델에 애니메이션이 있으면 컨트롤러 추가
                if (loadedModel.GetComponent<Animation>())
                    AnimationController.AddController(loadedModel);

                OnLoadedHandler?.Invoke(this, eventArgs);

                if (Parent != null)
                    resultData.transform.SetParent(Parent);

                currentKey = key;
                modelName.text = key;
            }
            else
            {
                Debug.Log("모델 로드 실패 resultData = null");
            }
        });
    }

    void UnLoadData(string targetKey)
    {
        Resources.UnloadUnusedAssets();
        OnReleasedHandler?.Invoke(this, loadedModel);
        AddressableAssetLoader.UnLoadData<GameObject>(targetKey);

        loadedModel = null;
    }

    public enum MessageLevel { Normal, Warning, Error }
    public void UserSystemMessage(string message, float fadeTime = 2f, MessageLevel level = MessageLevel.Normal)
    {
        if (level == MessageLevel.Normal)
        {
            levelIcon.sprite = normalSprite;
            levelIcon.color = normalColor;
        }
        else if (level == MessageLevel.Warning)
        {
            levelIcon.sprite = warningSprite;
            levelIcon.color = warningColor;
        }
        else if (level == MessageLevel.Error)
        {
            levelIcon.sprite = errorSprite;
            levelIcon.color = errorColor;
        }

        systemMessage.GetComponentInChildren<TMPro.TMP_Text>().text = message;

        levelIcon.DOKill();
        systemMessage.GetComponentInChildren<TMPro.TMP_Text>().DOKill();
        systemMessage.GetComponent<Image>().DOKill();
        systemMessage.GetComponentInChildren<TMPro.TMP_Text>().DOFade(1f, 0.2f);
        systemMessage.GetComponent<Image>().DOFade(0.8f, 0.2f);
        levelIcon.DOFade(1f, 0.2f);

        CancelInvoke();
        Invoke("HideMessage", fadeTime);
    }
    void HideMessage()
    {
        levelIcon.DOFade(0f, 0.2f);
        systemMessage.GetComponentInChildren<TMPro.TMP_Text>().DOFade(0f, 0.2f);
        systemMessage.GetComponent<Image>().DOFade(0f, 0.2f);
    }
}