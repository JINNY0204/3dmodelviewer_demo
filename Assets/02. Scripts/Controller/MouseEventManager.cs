using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using System.Linq;
using System.Runtime.InteropServices;

public class MouseEventManager : Singleton<MouseEventManager>
{
    public Animation mouseClickEffect;
    public Transform cursorTag;
    public Texture2D defaultCursor;
    static float clickStayTime;
    Canvas tagCanvas;

    protected override void Awake()
    {
        base.Awake();
        tagCanvas = cursorTag.GetComponentInParent<Canvas>();
    }
    void Update()
    {
        if (mouseClickEffect)
        {
            if (Input.GetMouseButtonDown(0))
            {
                mouseClickEffect.transform.position = Input.mousePosition;
                mouseClickEffect.Stop();
                mouseClickEffect.Play();
            }
        }
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            clickStayTime += Time.deltaTime;
        }
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            clickStayTime = 0f;
        }

        if (cursorTag && cursorTag.gameObject.activeSelf)
        {
            if (!IsPointerOverUIElement())
            {
                Vector3 pos = Input.mousePosition;
                pos.x += 50f * tagCanvas.scaleFactor;
                cursorTag.position = pos;
            }
        }
    }
    private void OnEnable()
    {
        if (defaultCursor)
            SetCursor(defaultCursor);
    }

    public static void SetCursor(Texture2D texture)
    {
        Cursor.SetCursor(texture, Vector2.zero, CursorMode.ForceSoftware);
    }

    public static bool IsClickDelayTimeout(float delayTime = 0.08f)
    {
        return clickStayTime < delayTime;
    }

    public static bool IsPointerOverUIElement()
    {
        var eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Where(r => r.gameObject.layer == 5).Count() > 0;
    }

    public void ShowTag(string tag)
    {
        if (cursorTag)
        {
            cursorTag.GetComponentInChildren<TMPro.TMP_Text>().text = tag;
            cursorTag.gameObject.SetActive(true);
        }
    }
    public void HideTag()
    {
        if (cursorTag)
            cursorTag.gameObject.SetActive(false);
    }



    [DllImport("__Internal")]
    private static extern void requestPointerLock();

    [DllImport("__Internal")]
    private static extern void exitPointerLock();

    public void RequestPointerLock()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        requestPointerLock();
#elif UNITY_EDITOR
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
#endif
    }

    public void ExitPointerLock()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
            exitPointerLock();
#elif UNITY_EDITOR
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
#endif
    }
}