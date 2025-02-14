using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SearchInputField : MonoBehaviour
{
    [Header("Search")]
    public TMPro.TMP_InputField searchInputfield;
    public Button searchBtn;
    public Button closeBtn;
    public bool case_Sensitive = false; //대소문자 구분 여부

    public void OnClick_SearchBtn()
    {
        searchInputfield.gameObject.SetActive(!searchInputfield.gameObject.activeSelf);
        if (searchInputfield.gameObject.activeSelf)
            searchBtn.GetComponent<Image>().color = new Color32(75, 75, 75, 255);
        else searchBtn.GetComponent<Image>().color = Color.white;
    }
    public void DisableSearchMode()
    {
        //closeBtn.onClick.Invoke();
        searchInputfield.text = string.Empty;
        searchBtn.GetComponent<Image>().color = Color.white;
    }

    public string InputTextToLower()
    {
        return searchInputfield.text.ToLower();
    }

}
