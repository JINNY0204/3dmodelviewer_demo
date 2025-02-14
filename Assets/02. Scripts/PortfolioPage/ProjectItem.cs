using DG.Tweening;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI;

public class ProjectItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string projectName;
    public bool allowHovering = true;
    private Image thumbnailImage;
    private Image[] images;
    private TMPro.TMP_Text[] textElements;

    private void Awake()
    {
        thumbnailImage = GetComponent<Image>();
        thumbnailImage.sprite = AtlasManager.Instance.projectThumbnailAtlas.GetSprite(projectName);

        Reset();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!allowHovering) return;

        for (int i = 0; i < images.Length; i++)
        {
            images[i].DOFillAmount(1f, 0.3f);
        }

        for (int i = 0;i < textElements.Length; i++)
        {
            textElements[i].DOColor(Color.white, 0.3f).SetDelay(0.2f);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!allowHovering) return;

        Color color = Color.white;
        color.a = 0f;

        for (int i = 0; i < images.Length; i++)
        {
            images[i].DOFillAmount(0f, 0.3f);
        }

        for (int i = 0; i < textElements.Length; i++)
        {
            textElements[i].DOKill();
            textElements[i].DOColor(color, 0.3f);
        }
    }

    private void Reset()
    {
        images = GetComponentsInChildren<Image>();
        textElements = GetComponentsInChildren<TMPro.TMP_Text>();
        //thumbnailImage = GetComponent<Image>();
        //thumbnailImage.sprite = Resources.Load<SpriteAtlas>("ProjectThumbnail").GetSprite(name);
        //projectName = name;
        if (!allowHovering) return;

        Color color = Color.white;
        color.a = 0f;
        for (int i = 0; i < images.Length; i++)
        {
            images[i].fillAmount = 0f;
        }

        for (int i = 0; i < textElements.Length; i++)
        {
            textElements[i].color = color;
        }
    }

    private void OnDestroy()
    {
        if (!allowHovering) return;

        if (images != null)
            for (int i = 0; i < images.Length; i++)
            {
                DOTween.Kill(images[i]);
            }

        if (textElements != null)
            for (int i = 0; i < textElements.Length; i++)
            {
                DOTween.Kill(textElements[i]);
            }
    }
}
