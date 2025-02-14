using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

public class VideoTest : MonoBehaviour
{
    VideoPlayer videoPlayer;
    public string clipName;
    public string videoUrl;
    public int requestTimeout = 120;

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        //videoUrl = Path.Combine(Application.streamingAssetsPath, clipName);
        StartCoroutine(GetRequest(videoUrl));
    }

    IEnumerator GetRequest(string url)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.timeout = requestTimeout;
        UnityWebRequestAsyncOperation op = request.SendWebRequest();
        op.completed += (result) =>
        {
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Request Failed :" + request.error);
            }
            else
            {
                Debug.Log("Request Success : " + request.url);
                videoPlayer.url = request.url;
                videoPlayer.Play();
            }
        };
        while (!op.isDone)
        {
            yield return null;
            Debug.Log("download process :  " + op.progress);
        }
    }
}
