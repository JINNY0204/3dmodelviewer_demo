using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleSwap : MonoBehaviour
{
    public Color selectedColor;
    Color normalColor;
    private void Awake()
    {
        normalColor = GetComponent<Image>().color;

        OnValueChanged();
        GetComponent<Toggle>().onValueChanged.AddListener(delegate { OnValueChanged(); });

    }

    public void OnValueChanged()
    {
        if (GetComponent<Toggle>().isOn)
        {
            GetComponent<Image>().color = selectedColor;
        }
        else GetComponent<Image>().color = normalColor;
    }
}
