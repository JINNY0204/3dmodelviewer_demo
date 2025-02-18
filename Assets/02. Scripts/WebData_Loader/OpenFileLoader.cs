using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class OpenFileLoader : Singleton<OpenFileLoader>
{

    [DllImport("__Internal")]
    private static extern void UploadFileJsLib(string gameObjectName, string methodName, string fileExtension);
    [DllImport("__Internal")]
    private static extern void UploadTextureJsLib(string gameObjectName, string methodName, int maxSize, string imageFormat);
    [DllImport("__Internal")]
    private static extern void DownloadFileJsLib(byte[] byteArray, int byteLength, string fileName);


    public enum ImageFormat
    {
        jpg,
        png
    }
    public enum FileExtension
    {
        zip,
        myownformat
    }

    private System.Action<Sprite> _onCompleteLoadImage;
    private bool _nonReadable = true;
    private Image _targetImage = null;

    #region Upload : 파일 브라우저에서 선택한 이미지 로드
    public void UploadTextureToImage(ImageFormat imageFormat, Image img, System.Action<Sprite> onCompleteLoad)
    {
        //Upload a Texture and set the image sprite
        UploadTexture(imageFormat, 1024, true, img, onCompleteLoad);
    }

    public void UploadTexture(ImageFormat imageFormat, int maxSize, bool nonReadable, Image targetImage = null, System.Action<Sprite> onCompleteLoadImage = null)
    {
        _nonReadable = nonReadable;
        _targetImage = targetImage;
        _onCompleteLoadImage = onCompleteLoadImage;
#if UNITY_EDITOR
        string[] allImages = new string[] { "images", imageFormat.ToString() };
        if (imageFormat == ImageFormat.jpg) allImages = new string[] { "jpg/png images", "png,jpg,jpeg" };
        string path = UnityEditor.EditorUtility.OpenFilePanelWithFilters("Load a texture...", "", allImages);
        StartCoroutine(LoadTexture(path));
#elif UNITY_WEBGL
        UploadTextureJsLib(gameObject.name, "LoadTexture", maxSize, imageFormat.ToString());
#endif
    }
    private IEnumerator LoadTexture(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            yield break;
        }

        using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url, _nonReadable);
        {
            yield return uwr.SendWebRequest();

            Sprite resultSprite = null;
            if (uwr.result != UnityWebRequest.Result.Success)
            {

            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);

                if (uwr.downloadedBytes > 2097152)
                {
                    ProcessManager.Instance.UserSystemMessage("2MB를 초과하는 파일은 업로드할 수 없습니다.\n<size=12>이미지파일 용량 : " + uwr.downloadedBytes + " bytes</size>", 5f, ProcessManager.MessageLevel.Warning);
                    yield break;
                }
                else if (_targetImage)
                {
                    resultSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
                    _targetImage.sprite = resultSprite;
                }
            }
            _onCompleteLoadImage?.Invoke(resultSprite);
        }
    }
    #endregion


    #region Download : 메모리 로드된 이미지를 파일로 export하여 사용자가 지정한 경로에 저장
    public void DownloadTexture(Texture2D tex, ImageFormat imageFormat)
    {
        byte[] texBytes;
        if (imageFormat == ImageFormat.png) texBytes = tex.EncodeToPNG();
        else texBytes = tex.EncodeToJPG();
        DownloadFile(texBytes, "texFileName", imageFormat.ToString());
    }

    public void DownloadFile(byte[] bytes, string fileName, string fileExtension)
    {
        if (fileName == "") fileName = "UnnamedFile";
#if UNITY_EDITOR
        string path = UnityEditor.EditorUtility.SaveFilePanel("Save file...", "", fileName, fileExtension);
        System.IO.File.WriteAllBytes(path, bytes);
        Debug.Log("File saved: " + path);
#elif UNITY_WEBGL
        DownloadFileJsLib(bytes, bytes.Length, fileName + "." + fileExtension);
#endif
    }
    #endregion
}