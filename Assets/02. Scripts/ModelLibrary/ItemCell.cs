using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;


public class ItemCell : MonoBehaviour
{
    public struct ModelData
    {
        byte fileSize;
        string label;
        string key;

        public ModelData(byte fileSize, string label, string key)
        {
            this.fileSize = fileSize;
            this.label = label;
            this.key = key;
        }
    }

    public Texture2D thumbnailTexture;
    public Image thumbnail;
    public TMP_Text label;
    public Button cellButton;
    public Button AddIMGButton;
    public ModelData data;
    public bool isEdited;


    public void AddIMG()
    {
        //Filebrowser열어서 사용자가 지정한 이미지 파일을 썸네일로 넣음
        OpenFileLoader.Instance.UploadTextureToImage(OpenFileLoader.ImageFormat.jpg, thumbnail, (sprite) => 
        {
            if (sprite != null)
            {
                sprite.name = label.text;
                isEdited = true;
            }
            else
            {
                isEdited = false;
            }
        });
    }
}