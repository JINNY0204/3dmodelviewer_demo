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

    [Header("������")]
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

        ShowMessage("<b><size=22><color=#FFFFFF>WASD</color></size></b> ��ư�� ���콺�� �̿��� �ƹ�Ÿ�� ���ϴ� ���� ��ġ�� �� �ֽ��ϴ�.");
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
            ProcessManager.Instance.UserSystemMessage("WALK ��带 �����մϴ�.");

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
            //���콺 �·Ḧ World ��ǥ��
            Vector3 targetPosition = Camera.main.ScreenToWorldPoint(mousePos);

            //������Ʈ ȭ�� �ȿ� ���α�
            Vector3 targetViewPos = Camera.main.WorldToViewportPoint(targetPosition); //������Ʈ ��ǥ(����)�� ��ũ�� ��ǥ��
            if (targetViewPos.x < 0f) targetViewPos.x = 0f;
            if (targetViewPos.y < 0f) targetViewPos.y = 0f;
            if (targetViewPos.x > 1f) targetViewPos.x = 1f;
            if (targetViewPos.y > 1f) targetViewPos.y = 1f;
            Vector3 resultPos = Camera.main.ViewportToWorldPoint(targetViewPos); //ȭ�鿡 ���������� ���ǹ� ���� �� �ٽ� ���� ��ǥ��

            //�ε巴�� �����̵��� Slerp ó��
            avatarInstance.transform.position = Vector3.Slerp(avatarInstance.transform.position, resultPos, 0.2f);

            if (Input.GetMouseButtonDown(0))
            {
                if (!usePointer || (usePointer && IsGround()))
                    if (!MouseEventManager.IsPointerOverUIElement())
                    {
                        if (usePointer)
                            pointer.SetActive(false);

                        MouseEventManager.Instance.RequestPointerLock();
                        ShowMessage("WALK ��带 �����Ϸ���, <b><size=22><color=#FFFFFF>ESC</color></size></b> ��ư�� ��������.");
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
                //���� �Ұ�
                sourceMat.SetColor("_BaseColor", Color.red);
                return false;
            }
            else
            {
                //���� ����
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
            ShowMessage("<b><size=22><color=#FFFFFF>WASD</color></size></b> ��ư�� ���콺�� �̿��� �ƹ�Ÿ�� ���ϴ� ���� ��ġ�� �� �ֽ��ϴ�.");

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