using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorPingpong : MonoBehaviour
{
    Color originalColor;
    public Color targetColor;
    public float duration;

    TMPro.TMP_Text TMP_targetText;
    Image targetImage;
    Text targetText;
    bool TMP_TextComp;
    bool imageComp;
    bool TextComp;

    Coroutine coroutine;

    private void Awake()
    {
        imageComp = GetComponent<Image>();
        TextComp = GetComponent<Text>();
        TMP_TextComp = GetComponent<TMPro.TMP_Text>();

        originalColor = GetColor();
    }
    private void OnEnable()
    {
        coroutine = StartCoroutine(ColorCor());
    }

    private void OnDisable()
    {
        StopCoroutine(coroutine);

        if (imageComp)
            GetComponent<Image>().color = originalColor;
        if (TextComp)
            GetComponent<Text>().color = originalColor;
        if (TMP_TextComp)
            GetComponent<TMPro.TMP_Text>().color = originalColor;
    }

    Color GetColor()
    {
        Color result = Color.white;

        if (imageComp)
        {
            targetImage = GetComponent<Image>();
            result = targetImage.color;
        }
        if (TextComp)
        {
            targetText = GetComponent<Text>();
            result = targetText.color;
        }
        if (TMP_TextComp)
        {
            TMP_targetText = GetComponent<TMPro.TMP_Text>();
            result = TMP_targetText.color;
        }

        return result;
    }
    IEnumerator ColorCor()
    {
        Color myColor = GetColor();

        while (true)
        {
            if (imageComp)
                targetImage.color = Color.Lerp(myColor, targetColor, Mathf.PingPong(Time.time, duration));
            if (TextComp)
                targetText.color = Color.Lerp(myColor, targetColor, Mathf.PingPong(Time.time, duration));
            if (TMP_TextComp)
                TMP_targetText.color = Color.Lerp(myColor, targetColor, Mathf.PingPong(Time.time, duration));
            yield return null;
        }
    }
}