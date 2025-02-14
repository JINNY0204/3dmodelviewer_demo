using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{
    public static Color selectedColor { get; private set; }
    public Toggle visualGraphic;
    public Texture2D spoidCursor;

    Color originalColor;

    private void Awake()
    {
        selectedColor = GetComponentsInChildren<Button>()[0].image.color;
        originalColor = visualGraphic.image.color;
        //gameObject.SetActive(false);
    }

    public void OnClick_Spoid()
    {
        Cursor.SetCursor(spoidCursor, new Vector2(0F, 25F), CursorMode.ForceSoftware);
    }

    public void OnClick_ColorButton(string htmlStr)
    {
        ColorUtility.TryParseHtmlString(htmlStr, out Color color);
        selectedColor = color;
        visualGraphic.image.color = color;
    }

    public void OnClick_TransparentButton(string htmlStr)
    {
        ColorUtility.TryParseHtmlString(htmlStr, out Color color);
        color.a = 0.1f;
    }

    public void Active(Toggle toggle)
    {
        gameObject.SetActive(toggle.isOn);
        if (toggle.isOn)
            visualGraphic.image.color = selectedColor;
        if (!toggle.isOn)
            visualGraphic.image.color = originalColor;
    }


    [Header("Spoid Palette")]
    [SerializeField] Image Palette;
    [SerializeField] RectTransform cursor;
    [SerializeField] Image button;
    public void GetColor(BaseEventData data)
    {
        PointerEventData pointer = data as PointerEventData;
        cursor.position = pointer.position;
        cursor.GetComponent<RectTransform>().anchoredPosition = ClampPointer(cursor.position);
        Color pickedColor = Palette.sprite.texture.GetPixel(
            (int)(cursor.localPosition.x * (Palette.sprite.texture.width / transform.GetChild(0).GetComponent<RectTransform>().rect.width)), 
            (int)(cursor.localPosition.y * (Palette.sprite.texture.height / transform.GetChild(0).GetComponent<RectTransform>().rect.height))
            );
        selectedColor = button.color = pickedColor;
    }
    public void OnSelect(BaseEventData data)
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
    }
    Vector2 ClampPointer(Vector2 pos)
    {
        Vector2 newPos = pos;
        float minWidthAbs = -Palette.GetComponent<RectTransform>().sizeDelta.x / 2f;
        float maxWidthAbs = Palette.GetComponent<RectTransform>().sizeDelta.x / 2f;
        float minHeightAbs = -Palette.GetComponent<RectTransform>().sizeDelta.y / 2f;
        float maxHeightAbs =Palette.GetComponent<RectTransform>().sizeDelta.y / 2f;

        newPos.x = Mathf.Clamp(newPos.x, minWidthAbs, maxWidthAbs);
        newPos.y = Mathf.Clamp(newPos.y, minHeightAbs, maxHeightAbs);

        return newPos;
    }
}