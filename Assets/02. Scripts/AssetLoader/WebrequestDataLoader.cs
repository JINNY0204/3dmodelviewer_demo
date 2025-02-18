using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System;

public class WebrequestLoader : MonoBehaviour
{
    public const string LIBRARY_THUMBNAIL = "LibraryThumbnail";
    public string path;

    public TMPro.TMP_Text debugTxt;

    void Start()
    {
        debugTxt.text = "dataPath : " + Application.dataPath;

        GetRequest(path, (UnityWebRequest request) => 
        {

        });
    }

    public void GetRequest(string uri, Action<UnityWebRequest> callback = null)
    {
        StartCoroutine(GetRequestCor(path, callback));
    }

    IEnumerator GetRequestCor(string uri, Action<UnityWebRequest> callback = null)
    {
        UnityWebRequest request = UnityWebRequest.Get(uri);
        request.certificateHandler = new AcceptCeritificates();

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            debugTxt.text += " //// request.error : " + request.error;
            Debug.Log("Request Failed :" + request.error);
        }
        else
        {
            debugTxt.text += " //// Request Success : " + request.url;
            Debug.Log("Request Success : " + request.url);
            callback?.Invoke(request);
        }
    }
}

public class AcceptCeritificates : CertificateHandler
{ 
    //private static string PUB_KEY = "temp";
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true; // (테스트용, 보안 매우 취약. 없는 거랑 마찬가지) 무조건 인증서를 허용하는 것으로 간주

        //X509Certificate2 certificate = new X509Certificate2(certificateData);
        //string pk = certificate.GetPublicKeyString();
        //Debug.Log(pk);
        //Debug.Log(pk.Equals(PUB_KEY));
        //if (pk.Equals(PUB_KEY))
        //    return true;
        //return false;
    }
}