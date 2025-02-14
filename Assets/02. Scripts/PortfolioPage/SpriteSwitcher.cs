using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpriteSwitcher : MonoBehaviour
{
    public Image targetImage;
    public Sprite[] sprites;
    public float interval;
    private int index;

    Coroutine Cor;

    private void OnEnable()
    {
        if (targetImage)
            targetImage.sprite = sprites[0];

        if (Cor == null)
            Cor = StartCoroutine(Play());
    }

    private void OnDisable()
    {
        if (Cor != null)
        {
            StopCoroutine(Cor);
            Cor = null;
            index = 0;
        }
    }

    private IEnumerator Play()
    {
        if (targetImage == null)
        {
            Debug.Log("no targetImages", gameObject);
            yield break;
        }
        if (sprites.Length == 0)
        {
            Debug.Log("no target sprites", gameObject);
            yield break;
        }

        WaitForSeconds intervalWait = new WaitForSeconds(interval);
        while (true)
        {
            yield return intervalWait;
            targetImage.sprite = sprites[index++];
            if (index > sprites.Length - 1)
            {
                index = 0;
            }
        }
    }
}
