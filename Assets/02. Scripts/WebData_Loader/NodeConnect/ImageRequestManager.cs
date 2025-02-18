using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ImageRequestManager : Singleton<ImageRequestManager>
{
    [SerializeField] private string SERVER_PATH; //IP + PORT  끝에 슬래쉬 없이.
    public readonly string UPLOAD_ENDPOINT = "api/uploadimage"; // 업로드
    public readonly string DOWNLOAD_ENDPOINT = "api/getimage"; // 다운로드

    protected override void OnDestroy()
    {
        uploadQueue.Clear();
        downloadQueue.Clear();
        base.OnDestroy();
    }

    public void DownloadImage(string imgName, Action<Texture> onCompleteReceive)
    {
        QueueImageDownload(imgName, onCompleteReceive);
    }

    public void UploadImage(Image targetImage, string fileName, Action<string> onCompleteUpload = null)
    {
        if (targetImage == null)
        {
            Debug.LogError("Target image is null");
            return;
        }

        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("File name is empty");
            return;
        }

        QueueImageUpload(targetImage, fileName, onCompleteUpload);
    }

    #region private functions
    private void Start()
    {
        StartCoroutine(ProcessUploadQueue());
        StartCoroutine(ProcessDownloadQueue());
    }
    private Texture2D CreateReadableTexture(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex); // 임시로 생성한 렌더텍스처 메모리 해제

        return readableText;
    }

    #region Upload Queue
    private Queue<ImageUploadRequest> uploadQueue = new Queue<ImageUploadRequest>();
    private bool isUploading = false;
    private void QueueImageUpload(Image targetImage, string fileName, Action<string> onCompleteUpload = null)
    {
        ImageUploadRequest uploadRequest = new ImageUploadRequest(targetImage, fileName, onCompleteUpload);
        uploadQueue.Enqueue(uploadRequest);
    }
    private IEnumerator UploadImage(ImageUploadRequest uploadRequest)
    {
        Image targetImage = uploadRequest.targetImage;
        string fileName = uploadRequest.fileName;
        Action<string> onCompleteUpload = uploadRequest.onCompleteUpload;

        var copy = CreateReadableTexture(targetImage.sprite.texture); //readable textrue로 복사
        var tex = ImageConversion.EncodeToPNG(copy);

        // copy 사용 후 해제
        Destroy(copy);

        List<IMultipartFormSection> formDataList = new List<IMultipartFormSection>();
        MultipartFormFileSection form = new MultipartFormFileSection("imgfile", tex, fileName + ".png", "image/png");
        formDataList.Add(form);

        // node.js 서버 엔드포인트로 이미지 post
        var path = string.Format("{0}/{1}", SERVER_PATH, UPLOAD_ENDPOINT);
        using (UnityWebRequest request = UnityWebRequest.Post(path, formDataList))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var result = request.downloadHandler.text;
                onCompleteUpload?.Invoke(result);
            }
            else
            {
                Debug.Log("No file uploaded : " + request.error);
            }
        }
        isUploading = false;
    }
    private IEnumerator ProcessUploadQueue()
    {
        while (true)
        {
            yield return null;

            if (!isUploading && uploadQueue.Count > 0)
            {
                isUploading = true;
                ImageUploadRequest uploadRequest = uploadQueue.Dequeue();
                yield return StartCoroutine(UploadImage(uploadRequest));
            }
        }
    }
    private class ImageUploadRequest
    {
        public Image targetImage;
        public string fileName;
        public System.Action<string> onCompleteUpload;

        public ImageUploadRequest(Image targetImage, string fileName, System.Action<string> onCompleteUpload)
        {
            this.targetImage = targetImage;
            this.fileName = fileName;
            this.onCompleteUpload = onCompleteUpload;
        }
    }
    #endregion

    #region Download Queue Pro
    private Queue<DownloadRequest> downloadQueue = new Queue<DownloadRequest>();
    private bool isDownloading = false;

    // 이미지 다운로드 요청을 큐에 추가하는 함수
    private void QueueImageDownload(string imgName, Action<Texture> onCompleteReceive)
    {
        DownloadRequest downloadRequest = new DownloadRequest(imgName, onCompleteReceive);
        downloadQueue.Enqueue(downloadRequest);

        // 큐가 비어있으면 즉시 처리를 시작
        if (!isDownloading)
        {
            StartCoroutine(ProcessDownloadQueue());
        }
    }

    // 이미지 다운로드 큐를 처리하는 코루틴
    private IEnumerator ProcessDownloadQueue()
    {
        isDownloading = true;

        while (downloadQueue.Count > 0)
        {
            DownloadRequest downloadRequest = downloadQueue.Dequeue();
            yield return StartCoroutine(DownloadImage(downloadRequest));
        }

        isDownloading = false;
    }

    // 개별 이미지 다운로드를 담당하는 함수
    private IEnumerator DownloadImage(DownloadRequest downloadRequest)
    {
        string imgName = downloadRequest.imgName;
        Action<Texture> onCompleteReceive = downloadRequest.onCompleteReceive;

        // 이미지 파일 이름을 포함한 node.js 서버 엔드포인트
        var path = string.Format("{0}/{1}/{2}", SERVER_PATH, DOWNLOAD_ENDPOINT, imgName + ".png");
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(path))
        {
            request.timeout = 30; // 30초로 설정
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (request.error.Contains("500"))
                    Debug.Log("[서버측 코드 에러] " + imgName);
                else if (request.error.Contains("404"))
                    Debug.Log("[파일 없음] " + imgName);
                else
                    Debug.Log("[기타 서버 에러] " + imgName);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                onCompleteReceive(texture);
            }
        }
    }

    // 이미지 다운로드 요청을 저장하는 클래스
    private class DownloadRequest
    {
        public string imgName;
        public System.Action<Texture> onCompleteReceive;

        public DownloadRequest(string imgName, Action<Texture> onCompleteReceive)
        {
            this.imgName = imgName;
            this.onCompleteReceive = onCompleteReceive;
        }
    }
    #endregion
    #endregion
}