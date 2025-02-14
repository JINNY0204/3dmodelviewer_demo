using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Youlsys.Calendar
{
    [RequireComponent(typeof(CalendarManager))]
    public class DatePicker : MonoBehaviour
    {
        public static (string, string) startEndDate;

        #region 
        public static bool canEdit { get; private set; }
        public List<Day> dayList = new List<Day>();
        public bool isStartSelect { get; private set; }
        CalendarManager calendarManager;
        public Transform tooltip;
        public Toggle datePickToggle;
        public Button goToSelectionBtn;
        public TMPro.TMP_Text messageText;
        public Color selectionColor;
        bool isPlaying;
        float originX;

        private void Start()
        {
            calendarManager = GetComponent<CalendarManager>();
            startEndDate = (DateTime.Today.ToString("yyyy-MM-dd"), DateTime.Today.ToString("yyyy-MM-dd"));
            datePickToggle.onValueChanged.AddListener(delegate { OnClick_DatePickToggle(datePickToggle); });
            originX = datePickToggle.GetComponent<RectTransform>().anchoredPosition.x;
            ResetSelection();
        }

        private void OnDisable()
        {
            datePickToggle.isOn = false;
            OnClick_DatePickToggle(datePickToggle);
            tooltip.gameObject.SetActive(false);
            isStartSelect = false;
            canEdit = false;
        }

        private void Update()
        {
            if (tooltip.gameObject.activeSelf && canEdit)
            {
                Vector3 pos = Input.mousePosition;
                pos.x += 40f;
                pos.y += 40f;
                tooltip.position = pos;
            }
        }

        public void OnCompleteSelection(string start, string end)
        {
            startEndDate = (start, end);

            if (!string.IsNullOrEmpty(start) && string.IsNullOrEmpty(end)) //start ��¥�� ���� �� üũ�� ���
            {
                startEndDate = (start, start);
                messageText.text = string.Format("���� & ���� ��¥ : {0}", start);
            }
            else if (string.IsNullOrEmpty(start) && string.IsNullOrEmpty(end)) //�ƹ��͵� üũ���� ���� ���
            {
                messageText.text = "";
            }
            else if (!string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end)) //start, end ��¥ ��� ���� �� üũ�� ���
            {
                startEndDate = (start, end);
                if (start.Equals(end))
                    messageText.text = string.Format("���� & ���� ��¥ : {0}", start);
                else
                    messageText.text = string.Format("���۳�¥ : {0}   ���ᳯ¥ : {1}", start, end);
            }
            tooltip.gameObject.SetActive(false);
        }
        public void ResetSelection()
        {
            datePickToggle.isOn = false;
            OnCompleteSelection(string.Empty, string.Empty);
        }

        public void OnClick_DatePickToggle(Toggle toggle)
        {
            if (isPlaying) return;

            isStartSelect = false;
            canEdit = toggle.isOn;
            tooltip.gameObject.SetActive(toggle.isOn);

            SetAnimation(toggle.isOn);

            if (toggle.isOn) //��������
            {
                OnCompleteSelection(string.Empty, string.Empty);

                for (int i = 0; i < dayList.Count; i++)
                {
                    dayList[i].Initialize();

                    int index = i;
                    Button dayBtn = dayList[index].GetComponent<Button>();
                    dayBtn.onClick.RemoveAllListeners();
                    dayBtn.onClick.AddListener(() => OnSelect(dayList[index]));
                    tooltip.GetComponentInChildren<Text>().text = "���۳�¥ ����";
                }
            }
            else //�����Ϸ� üũ
            {
                OnCompleteSelection(startEndDate.Item1, startEndDate.Item2);

                for (int i = 0; i < dayList.Count; i++)
                {
                    int index = i;
                    Button dayBtn = dayList[index].GetComponent<Button>();
                    dayBtn.onClick.RemoveAllListeners();
                    dayBtn.onClick.AddListener(delegate { Day.OnSelect(dayList[index].dayItem.dateTime); });
                }
            }
        }
        
        void SetAnimation(bool value)
        {
            isPlaying = true;
            calendarManager.resetButton.gameObject.SetActive(!value);
            calendarManager.todayButton.gameObject.SetActive(!value);
            float end = value ? -50f : originX;
            datePickToggle.GetComponent<RectTransform>().DOAnchorPosX(end, 0.2f).SetDelay(0.2f).OnComplete( delegate { isPlaying = false; });
        }

        public void OnSelect(Day selectedDay)
        {
            for (int i = 0; i < dayList.Count; i++)
                dayList[i].Initialize();

            if (!isStartSelect) //Start��¥ ����
            {
                isStartSelect = true;
               //startDate = selectedDay.dayItem.dateTime;
                startEndDate = (selectedDay.dayItem.dateStr, null);
                selectedDay.GetComponent<Image>().color = selectionColor;
                tooltip.GetComponentInChildren<Text>().text = "����¥ ����";
            }
            else //End��¥ ����
            {
                if (DateTime.Parse(startEndDate.Item1) <= selectedDay.dayItem.dateTime)
                {
                    isStartSelect = false;
                    //endDate = selectedDay.dayItem.dateTime;
                    startEndDate = (startEndDate.Item1, selectedDay.dayItem.dateStr);
                    tooltip.GetComponentInChildren<Text>().text = "üũ�Ͽ� ����";

                    for (int i = 0; i < dayList.Count; i++)
                        if (dayList[i].dayItem.dateTime >= DateTime.Parse(startEndDate.Item1) && dayList[i].dayItem.dateTime <= DateTime.Parse(startEndDate.Item2))
                        {
                            dayList[i].GetComponent<Image>().color = selectionColor;
                        }
                }
                else
                {
                    isStartSelect = true;
                    startEndDate = (selectedDay.dayItem.dateStr, null);
                    selectedDay.GetComponent<Image>().color = selectionColor;
                    tooltip.GetComponentInChildren<Text>().text = "����¥ ����";
                }
            }
        }
    }
    #endregion
}