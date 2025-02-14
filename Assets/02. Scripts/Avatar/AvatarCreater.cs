using Cinemachine;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AvatarCreater : MonoBehaviour
{
    bool isHold;
    public GameObject messageCanvas;
    public GameObject menuCanvas;
    public Toggle createToggle;
    GameObject avatarInstance;

    [Header("감지기")]
    public bool usePointer;
    public GameObject pointer;
    public Material sourceMat;
    
    private void Awake()
    {
        createToggle.onValueChanged.AddListener(delegate { OnToggleChanged(createToggle); });
    }
    
    void OnToggleChanged(Toggle toggle)
    {
        if (toggle.isOn)
        {
            CreateAvatar();
        }
        else
        {
            DestroyAvatar();
        }
    }

    void CreateAvatar()
    {
        ModelModifier.Instance.OnClick_ResetBtn(false);
        UILayoutManager.Instance.flipToggle.isOn = false;
        CameraModeController.Instance.firstPersonToggle.isOn = true;
        Camera.main.DOFieldOfView(70f, 0.4f);

        ShowMessage("<b><size=22><color=#FFFFFF>WASD</color></size></b> 버튼과 마우스를 이용해 아바타를 원하는 곳에 배치할 수 있습니다.");
        //MouseEventManager.Instance.RequestPointerLock();

        avatarInstance = Instantiate(Resources.Load<GameObject>("Avatar_Man"));
        avatarInstance.GetComponent<CharacterController>().enabled = false;
        avatarInstance.GetComponentInChildren<CinemachineVirtualCamera>().enabled = false;

        isHold = true;
    }

    void DestroyAvatar()
    {
        if (avatarInstance)
        {
            ProcessManager.Instance.UserSystemMessage("WALK 모드를 종료합니다.");

            if (usePointer)
                pointer.SetActive(false);

            HideMessage();
            MouseEventManager.Instance.ExitPointerLock();

            Destroy(avatarInstance);
            UILayoutManager.Instance.flipToggle.isOn = true;
            Camera.main.DOFieldOfView(60f, 0.4f);
            Camera.main.nearClipPlane = 0.01f;
        }
    }

    private void Update()
    {
        if (isHold && avatarInstance)
        {
            if (usePointer) IsGround();

            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.nearClipPlane + 5f;
            //마우스 좌료를 World 좌표로
            Vector3 targetPosition = Camera.main.ScreenToWorldPoint(mousePos);

            //오브젝트 화면 안에 가두기
            Vector3 targetViewPos = Camera.main.WorldToViewportPoint(targetPosition); //오브젝트 좌표(월드)를 스크린 좌표로
            if (targetViewPos.x < 0f) targetViewPos.x = 0f;
            if (targetViewPos.y < 0f) targetViewPos.y = 0f;
            if (targetViewPos.x > 1f) targetViewPos.x = 1f;
            if (targetViewPos.y > 1f) targetViewPos.y = 1f;
            Vector3 resultPos = Camera.main.ViewportToWorldPoint(targetViewPos); //화면에 가둬지도록 조건문 설정 후 다시 월드 좌표로

            //부드럽게 움직이도록 Slerp 처리
            avatarInstance.transform.position = Vector3.Slerp(avatarInstance.transform.position, resultPos, 0.2f);

            if (Input.GetMouseButtonDown(0))
            {
                if (!usePointer || (usePointer && IsGround()))
                    if (!MouseEventManager.IsPointerOverUIElement())
                    {
                        if (usePointer)
                            pointer.SetActive(false);

                        MouseEventManager.Instance.RequestPointerLock();
                        ShowMessage("WALK 모드를 종료하려면, <b><size=22><color=#FFFFFF>ESC</color></size></b> 버튼을 누르세요.");
                        avatarInstance.GetComponentInChildren<CinemachineVirtualCamera>().enabled = true;
                        avatarInstance.GetComponent<CharacterController>().enabled = true;
                        isHold = false;
                    }
            }
        }
        else if (!isHold && avatarInstance)
        {
            if (Input.GetMouseButtonUp(1))
            {
                if (!menuCanvas.activeSelf)
                {
                    menuCanvas.SetActive(true);
                    avatarInstance.GetComponentInChildren<CinemachineVirtualCamera>().enabled = false;
                    MouseEventManager.Instance.ExitPointerLock();
                }
            }
        }

        if (createToggle.isOn && Input.GetKeyUp(KeyCode.Escape))
        {
            createToggle.isOn = false;
        }
    }

    bool IsGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(avatarInstance.transform.position, -avatarInstance.transform.up, out hit))
        {
            if (!pointer.activeSelf)
                pointer.SetActive(true);

            Vector3 pos = avatarInstance.transform.position;
            pos.y = hit.point.y;
            pointer.transform.position = pos;

            Quaternion rot = Quaternion.FromToRotation(Vector3.up, hit.normal);
            pointer.transform.rotation = rot;
            if (hit.normal.y < 0.8f)
            {
                //생성 불가
                sourceMat.SetColor("_BaseColor", Color.red);
                return false;
            }
            else
            {
                //생성 가능
                sourceMat.SetColor("_BaseColor", new Color32(0, 201, 255, 255));
                return true;
            }
        }
        else
        {
            if (pointer.activeSelf)
                pointer.SetActive(false);
            return false;
        }
    }

    public void ShowMessage(string msg)
    {
        messageCanvas.SetActive(true);
        messageCanvas.GetComponentInChildren<TMPro.TMP_Text>().text = msg;
    }
    public void HideMessage()
    {
        messageCanvas.SetActive(false);
    }

    #region Menu Button Event
    public void OnClick_RelocationBtn()
    {
        if (!isHold && avatarInstance)
        {
            menuCanvas.SetActive(false);

            Camera.main.DOFieldOfView(70f, 0.4f);
            ShowMessage("<b><size=22><color=#FFFFFF>WASD</color></size></b> 버튼과 마우스를 이용해 아바타를 원하는 곳에 배치할 수 있습니다.");

            avatarInstance.GetComponent<CharacterController>().enabled = false;
            avatarInstance.GetComponentInChildren<CinemachineVirtualCamera>().enabled = false;

            isHold = true;
        }
    }
    public void OnClick_EnableToolbarBtn()
    {
        if (!isHold && avatarInstance)
        {
            menuCanvas.SetActive(false);
        }
    }
    public void OnClick_QuitBtn()
    {
        if (!isHold && avatarInstance)
        {
            MouseEventManager.Instance.ExitPointerLock();
            menuCanvas.SetActive(false);
            createToggle.isOn = false;
        }
    }
    public void OnClick_CloseBtn()
    {
        MouseEventManager.Instance.RequestPointerLock();
        menuCanvas.SetActive(false);
        avatarInstance.GetComponentInChildren<CinemachineVirtualCamera>().enabled = true;
    }
    #endregion
}