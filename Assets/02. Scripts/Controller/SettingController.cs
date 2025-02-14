using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SettingController : MonoBehaviour
{
    #region 속성
    public GameObject window;
    public GameObject messagePanel;

    [Header("Rollback")]
    bool fullScreen;
    int currentQualityLevel;

    float currentMoveSpeed;
    float currentPanningSpeed;
    float currentRotationSpeed;
    float currentZoomSpeed;

    float originMoveSpeed;
    float originPanningSpeed;
    float originRotationSpeed;
    float originZoomSpeed;

    [Header("Graphic")]
    public Toggle fullScreenToggle;
    public Dropdown resolutionDropdown;
    public Dropdown qualityDropdown;
    public Dropdown textureDropdown;
    public Dropdown antiAliasingDropdown;

    [Header("Control")]
    public Slider moveSlider;
    public Slider sensitivitySlider;

    Resolution[] resolutions;
    int prevResolutionIndex;
    int currenetResolutionIndex = 0;

    CameraController camController;
    #endregion
    private void Start()
    {
        camController = Camera.main.GetComponent<CameraController>();

        InitResolution();
        InitQuality();
        InitCameraProp();
        LoadSettings(currenetResolutionIndex);
    }

    public void OpenSetting()
    {
        SaveCurrentSetting();
        window.SetActive(true);
        window.GetComponent<Image>().DOFade(0.7f, 0.2f);
    }

    //변경 전 세팅정보 저장
    void SaveCurrentSetting()
    {
        fullScreen = Screen.fullScreen;
        prevResolutionIndex = resolutionDropdown.value;
        currentQualityLevel = QualitySettings.GetQualityLevel();

        if (camController)
        {
            currentMoveSpeed = camController.moveSpeed;
            currentPanningSpeed = camController.panningSpeed;
            currentRotationSpeed = camController.rotationSpeed;
            currentZoomSpeed = camController.zoomSpeed;
        }
    }

    #region 설정 옵션 세팅
    void InitResolution()
    {
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        resolutions = Screen.resolutions;

        if (resolutions.Length > 0)
        {
            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = resolutions[i].width + " x " + resolutions[i].height;
                options.Add(option);

                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height)
                    currenetResolutionIndex = i;
            }
        }
        else
        {
            string option = "지원되지 않는 플랫폼";
            options.Add(option);
            resolutionDropdown.interactable = false;
            fullScreenToggle.interactable = false;
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.RefreshShownValue();
    }

    void InitQuality()
    {
        List<string> options = new List<string>();
        string[] qualityNames = QualitySettings.names;
        for (int i = 0; i < qualityNames.Length; i++)
        {
            options.Add(qualityNames[i]);
        }
        qualityDropdown.AddOptions(options);
        qualityDropdown.RefreshShownValue();
    }

    void InitCameraProp()
    {
        if (camController)
        {
            originMoveSpeed = camController.moveSpeed;
            originPanningSpeed = camController.panningSpeed;
            originRotationSpeed = camController.rotationSpeed;
            originZoomSpeed = camController.zoomSpeed;
        }
    }
    #endregion

    public void SetFullScreen()
    {
        Screen.fullScreen = fullScreenToggle.isOn;
        //if (fullScreenToggle.isOn)
        //{
        //    fullScreenToggle.GetComponentInChildren<Text>().text = "전체 화면";
        //}
        //else
        //    fullScreenToggle.GetComponentInChildren<Text>().text = "창 모드";
    }

    public void SetResolution(int index)
    {
        Resolution resolution = resolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
    }

    void LoadSettings(int currentResolutionIndex)
    {
        if (PlayerPrefs.HasKey("FullscreenPreference"))
            Screen.fullScreen = Convert.ToBoolean(PlayerPrefs.GetInt("FullscreenPreference"));

        if (qualityDropdown)
        {
            if (PlayerPrefs.HasKey("QualitySettingPreference"))
                qualityDropdown.value =
                             PlayerPrefs.GetInt("QualitySettingPreference");
            else
                qualityDropdown.value = 0;
        }
        if (resolutionDropdown)
        {
            if (PlayerPrefs.HasKey("ResolutionPreference"))
                resolutionDropdown.value =
                             PlayerPrefs.GetInt("ResolutionPreference");
            else
                resolutionDropdown.value = currentResolutionIndex;
        }
        if (moveSlider)
        {
            if (PlayerPrefs.HasKey("MoveSpeedPreference"))
                moveSlider.value = PlayerPrefs.GetFloat("MoveSpeedPreference");
            else
                moveSlider.value = moveSlider.maxValue;

            if (camController)
                camController.moveSpeed = originMoveSpeed * moveSlider.value;
        }

        if (sensitivitySlider)
        {
            if (PlayerPrefs.HasKey("MouseSensitivityPreference"))
                sensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivityPreference");
            else
                sensitivitySlider.value = sensitivitySlider.maxValue;

            if (camController)
            {
                camController.panningSpeed = originPanningSpeed * sensitivitySlider.value;
                camController.rotationSpeed = originRotationSpeed * sensitivitySlider.value;
                camController.zoomSpeed = originZoomSpeed * sensitivitySlider.value;
            }
        }
        //if (textureDropdown)
        //{
        //    if (PlayerPrefs.HasKey("TextureQualityPreference"))
        //        textureDropdown.value =
        //                     PlayerPrefs.GetInt("TextureQualityPreference");
        //    else
        //        textureDropdown.value = 0;
        //}

        //if (antiAliasingDropdown)
        //{
        //    if (PlayerPrefs.HasKey("AntiAliasingPreference"))
        //        antiAliasingDropdown.value =
        //                     PlayerPrefs.GetInt("AntiAliasingPreference");
        //    else
        //        antiAliasingDropdown.value = 1;
        //}
    }

    public void OnClick_SaveBtn()
    {
        if (qualityDropdown)
            PlayerPrefs.SetInt("QualitySettingPreference", qualityDropdown.value);
        if (resolutionDropdown)
        {
            PlayerPrefs.SetInt("ResolutionPreference", resolutionDropdown.value);
            currenetResolutionIndex = resolutionDropdown.value;
        }
        if (moveSlider)
        {
            PlayerPrefs.SetFloat("MoveSpeedPreference", moveSlider.value);
            if (camController)
                camController.moveSpeed = originMoveSpeed * sensitivitySlider.value;
        }
        if (sensitivitySlider)
        {
            PlayerPrefs.SetFloat("MouseSensitivityPreference", sensitivitySlider.value);
            if (camController)
            {
                camController.panningSpeed = originPanningSpeed * sensitivitySlider.value;
                camController.rotationSpeed = originRotationSpeed * sensitivitySlider.value;
                camController.zoomSpeed = originZoomSpeed * sensitivitySlider.value;
            }
        }

        //if (textureDropdown)
        //    PlayerPrefs.SetInt("TextureQualityPreference",
        //           textureDropdown.value);
        //if (antiAliasingDropdown)
        //    PlayerPrefs.SetInt("AntiAliasingPreference",
        //           antiAliasingDropdown.value);
        //    PlayerPrefs.SetInt("FullscreenPreference",
        //           Convert.ToInt32(Screen.fullScreen));

        //window.SetActive(false);
        //window.GetComponent<PopupTweenEffect>().Close(delegate { window.SetActive(false); });    

        window.GetComponent<Image>().DOFade(0, 0.2f).OnComplete(delegate
        {
            window.SetActive(false);
            ProcessManager.Instance.UserSystemMessage("환경설정이 저장되었습니다.");
        });
    }

    #region 메세지패널 이벤트
    //Setting/MessagePanel/OK Btn 버튼 이벤트
    public void OnClick_YesBtn()
    {
        CancelAll();
        window.GetComponent<Image>().DOFade(0, 0.2f).OnComplete(delegate
        {
            messagePanel.SetActive(false);
            window.SetActive(false);
        });
    }

    public void OnClick_NoBtn()
    {
        messagePanel.SetActive(false);
    }

    #endregion
    public void CancelAll()
    {
        fullScreenToggle.isOn = Screen.fullScreen;
        resolutionDropdown.value = prevResolutionIndex;
        qualityDropdown.value = currentQualityLevel;
        CameraController CM = Camera.main.GetComponent<CameraController>();
        CM.moveSpeed = currentMoveSpeed;
        CM.panningSpeed = currentPanningSpeed;
        CM.rotationSpeed = currentRotationSpeed;
        CM.zoomSpeed = currentZoomSpeed;
    }
}