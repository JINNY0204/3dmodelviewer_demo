using System.Runtime.InteropServices;
using System.Collections;
using UnityEngine;
using System.IO;

public class CaptureScreen : MonoBehaviour
{
    public enum ImageFormat
    {
        png, jpg
    }
    public RenderTexture renderTexture;
    //string path = "";

    public void OnClick_CaptureBtn()
    {
        gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(OpenDialogAndSave());
    }
    public IEnumerator OpenDialogAndSave(bool contain_UI = false)
    {
        yield return new WaitForEndOfFrame();

        Texture2D screenShot = null;
        if (contain_UI)
            screenShot = ScreenCapture.CaptureScreenshotAsTexture();
        else
        {
            screenShot = new Texture2D(renderTexture.width, renderTexture.height);
            RenderTexture.active = renderTexture;
            screenShot.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            screenShot.Apply();
        }

        try
        {

            OpenFileLoader.Instance.DownloadTexture(screenShot, OpenFileLoader.ImageFormat.png);

            //string fileName = System.DateTime.Now.ToString("스크린샷");
            //byte[] pngData = screenShot.EncodeToPNG();
//#if UNITY_EDITOR
//            path = UnityEditor.EditorUtility.SaveFilePanel("이미지 파일 저장", "", fileName, "png");
//            File.WriteAllBytes(path, pngData);
//            Debug.Log("File Saved : " + path);
//#elif UNITY_WEBGL
//            DownloadFileJsLib(pngData, pngData.Length, fileName + ".png");
//#endif
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("이미지 파일 저장 실패 : " + ex);
        }

        Destroy(screenShot);
        gameObject.SetActive(false);
    }

//#if UNITY_WEBGL
//    [DllImport("__Internal")]
//    private static extern void DownloadFileJsLib(byte[] byteArray, int byteLength, string fileName);
//#endif
    #region Save at persistantDataPath
    public IEnumerator TakeScreenShot()
    {
        yield return new WaitForEndOfFrame();

        var oldRT = RenderTexture.active;

        var texture = new Texture2D(renderTexture.width, renderTexture.height);
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        string fileName = System.DateTime.Today.ToString("d") + ".png";
        string folderPath = Path.Combine(Application.persistentDataPath, "Snapshot");
        string filePath = Path.Combine(folderPath, fileName);

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        yield return new WaitUntil(() => Directory.Exists(folderPath));

        string message = null;
        try
        {
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(filePath, bytes);

            message = "저장 완료\n" + filePath;
        }
        catch (System.Exception e)
        {
            RenderTexture.active = oldRT;
            Debug.Log("Error : " + e);

            message = "저장 실패\n" + e;
        }
        //infoPanel.GetComponent<PopupTweenEffect>().Open();
        //infoPanel.GetComponentInChildren<TMPro.TMP_Text>().text = message;
        //yield return new WaitForSeconds(2f);
        //infoPanel.GetComponent<PopupTweenEffect>().Close(delegate { gameObject.SetActive(false); });
    }
    #endregion
}