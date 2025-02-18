using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Youlsys.Calendar
{
	public class Day : MonoBehaviour
	{
		public struct DayItem
		{
			public DateTime dateTime;
			public string dateStr;
			public int year;
			public int month;
			public int day;
		}
		public DayItem dayItem;

		public TMPro.TMP_Text dayLabel;
		[Header("Mark")]
		public GameObject todayMark;
		[Header("Color")]
		public Color todayColor;
		public Color saturday;
		public Color sunday;
		public Color weekday;
		Color myOriginColor;

        private void Awake()
        {
			myOriginColor = GetComponent<Image>().color;
        }

        public void Set(DateTime dateTIme, DatePicker datePicker)
        {
			dayItem.dateStr = dateTIme.ToString("yyyy-MM-dd");
			dayItem.dateTime = DateTime.Parse(dayItem.dateStr);
			dayItem.year = dateTIme.Year;
			dayItem.month = dateTIme.Month;
			dayItem.day = dateTIme.Day;

			if (dayItem.dateTime.DayOfWeek == DayOfWeek.Saturday)
				dayLabel.color = saturday;
			else if (dayItem.dateTime.DayOfWeek == DayOfWeek.Sunday)
				dayLabel.color = sunday;
			else dayLabel.color = weekday;

			if (dayItem.dateTime == DateTime.Today)
			{
				//GetComponent<Image>().color = todayColor;
				todayMark.SetActive(true);
				todayMark.GetComponent<Image>().color = todayColor;
			}

			dayLabel.text = dayItem.day.ToString();

			GetComponent<Button>().onClick.AddListener(() => OnSelect(dayItem.dateTime));


			if (datePicker)
				SetSelectionState(datePicker);
		}

		public void SetSelectionState(DatePicker datePicker)
        {
			datePicker.dayList.Add(this);

			string startPick = DatePicker.startEndDate.Item1;
			string endPick = DatePicker.startEndDate.Item2;

			//날짜 선택 모드일때,
			if (DatePicker.canEdit)
			{
				GetComponent<Button>().onClick.RemoveAllListeners();
				GetComponent<Button>().onClick.AddListener(() => datePicker.OnSelect(this));

				//Start 날짜만 선택한 상황이라면,
				if (!string.IsNullOrEmpty(startPick) && string.IsNullOrEmpty(endPick))
				{
					//선택한 Start 날짜 표시하기
					if (dayItem.dateTime == DateTime.Parse(startPick))
					{
						GetComponent<Image>().color = datePicker.selectionColor;
					}
				}
				//Start, End 모두 선택한 상황이라면,
				else if (!string.IsNullOrEmpty(startPick) && !string.IsNullOrEmpty(endPick))
				{
					if (dayItem.dateTime >= DateTime.Parse(startPick) && dayItem.dateTime <= DateTime.Parse(endPick))
					{
						GetComponent<Image>().color = datePicker.selectionColor;
					}
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(startPick) && !string.IsNullOrEmpty(endPick))
					if (dayItem.dateTime >= DateTime.Parse(startPick) && dayItem.dateTime <= DateTime.Parse(endPick))
					{
						GetComponent<Image>().color = datePicker.selectionColor;
					}
			}
		}

		public static void OnSelect(DateTime dateTime)
        {
			Debug.Log(dateTime);
        }

		public void Initialize()
        {
			if (dayItem.dateTime == DateTime.Today)
			{
				//image.color = todayColor;
				todayMark.SetActive(true);
			}
			else
			{
				todayMark.SetActive(false);
				GetComponent<Image>().color = myOriginColor;
			}
		}
	}
}
