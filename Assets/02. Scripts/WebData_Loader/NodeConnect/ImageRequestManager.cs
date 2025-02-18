using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ImageRequestManager : Singleton<ImageRequestManager>
{
    [SerializeField] private string SERVER_PATH; //IP + PORT  ���� ������ ����.
    public readonly string UPLOAD_ENDPOINT = "api/uploadimage"; // ���ε�
    public readonly string DOWNLOAD_ENDPOINT = "api/getimage"; // �ٿ�ε�

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
        RenderTexture.ReleaseTemporary(renderTex); // �ӽ÷� ������ �����ؽ�ó �޸� ����

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

        var copy = CreateReadableTexture(targetImage.sprite.texture); //readable textrue�� ����
        var tex = ImageConversion.EncodeToPNG(copy);

        // copy ��� �� ����
        Destroy(copy);

        List<IMultipartFormSection> formDataList = new List<IMultipartFormSection>();
        MultipartFormFileSection form = new MultipartFormFileSection("imgfile", tex, fileName + ".png", "image/png");
        formDataList.Add(form);

        // node.js ���� ��������Ʈ�� �̹��� post
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

    // �̹��� �ٿ�ε� ��û�� ť�� �߰��ϴ� �Լ�
    private void QueueImageDownload(string imgName, Action<Texture> onCompleteReceive)
    {
        DownloadRequest downloadRequest = new DownloadRequest(imgName, onCompleteReceive);
        downloadQueue.Enqueue(downloadRequest);

        // ť�� ��������� ��� ó���� ����
        if (!isDownloading)
        {
            StartCoroutine(ProcessDownloadQueue());
        }
    }

    // �̹��� �ٿ�ε� ť�� ó���ϴ� �ڷ�ƾ
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

    // ���� �̹��� �ٿ�ε带 ����ϴ� �Լ�
    private IEnumerator DownloadImage(DownloadRequest downloadRequest)
    {
        string imgName = downloadRequest.imgName;
        Action<Texture> onCompleteReceive = downloadRequest.onCompleteReceive;

        // �̹��� ���� �̸��� ������ node.js ���� ��������Ʈ
        var path = string.Format("{0}/{1}/{2}", SERVER_PATH, DOWNLOAD_ENDPOINT, imgName + ".png");
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(path))
        {
            request.timeout = 30; // 30�ʷ� ����
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (request.error.Contains("500"))
                    Debug.Log("[������ �ڵ� ����] " + imgName);
                else if (request.error.Contains("404"))
                    Debug.Log("[���� ����] " + imgName);
                else
                    Debug.Log("[��Ÿ ���� ����] " + imgName);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                onCompleteReceive(texture);
            }
        }
    }

    // �̹��� �ٿ�ε� ��û�� �����ϴ� Ŭ����
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