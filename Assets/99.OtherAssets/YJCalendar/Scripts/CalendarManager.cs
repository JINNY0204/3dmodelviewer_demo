using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace Youlsys.Calendar
{
	public class CalendarManager : MonoBehaviour
	{
		#region 속성
		public DatePicker datePicker;
		public Transform group;
		[Header("Header 속성")]
		public TMPro.TMP_Text headerText;
		public Button nextMonthBtn;
		public Button prevMonthBtn;
		public Button todayButton;
		public Button resetButton;

		[Header("Body 속성")]
		public Transform bodyContainer;
		public GameObject dayPrefab;
		public GameObject placeHolderPrefab;

		List<GameObject> cells;
		public DateTime targetDateTime;
		CultureInfo cultureInfo;
		#endregion

		private void Start()
		{
			targetDateTime = DateTime.Today;
			cultureInfo = new CultureInfo("ko-KR");
			Initialize(targetDateTime.Year, targetDateTime.Month);

			prevMonthBtn.onClick.AddListener(() => OnClick_MoveMonthButton("Prev"));
			nextMonthBtn.onClick.AddListener(() => OnClick_MoveMonthButton("Next"));
			todayButton.onClick.AddListener(() => OnClick_TodayButton());
			resetButton.onClick.AddListener(() => OnClick_ResetButton());
		}
		public void OnClick_ResetButton()
        {
			if (datePicker)
				datePicker.ResetSelection();

			targetDateTime = DateTime.Today;
			Initialize(targetDateTime.Year, targetDateTime.Month);
		}
		public void OnClick_TodayButton()
		{
			targetDateTime = DateTime.Today;
			Initialize(targetDateTime.Year, targetDateTime.Month);
		}
		public void OnClick_MoveMonthButton(string param)
		{
			if (param == "Prev")
				targetDateTime = targetDateTime.AddMonths(-1);
			else if (param == "Next")
				targetDateTime = targetDateTime.AddMonths(1);

			Initialize(targetDateTime.Year, targetDateTime.Month);
		}
		public void Initialize(int year, int month)
		{
			headerText.text = year + " " + cultureInfo.DateTimeFormat.GetMonthName(month);
			var dateTime = new DateTime(year, month, 1);
			var daysInMonth = DateTime.DaysInMonth(year, month);

			var dayOfWeek = (int)dateTime.DayOfWeek;
			var size = (dayOfWeek + daysInMonth) / 7;

			if ((dayOfWeek + daysInMonth) % 7 > 0)
				size++;

			var arr = new int[size * 7];

			for (var i = 0; i < daysInMonth; i++)
				arr[dayOfWeek + i] = i + 1;

			if (cells == null)
				cells = new List<GameObject>();

			foreach (var c in cells)
				Destroy(c);

			cells.Clear();

			if (datePicker)
				datePicker.dayList.Clear();

			foreach (var day in arr)
			{
				GameObject buttonInstance = null;
				if (day == 0)
					buttonInstance = Instantiate(placeHolderPrefab, bodyContainer);
				else
				{
					DateTime date = new DateTime(year, month, day);
					buttonInstance = Instantiate(dayPrefab, bodyContainer);
					Day Day = buttonInstance.GetComponent<Day>();
					Day.Set(date, datePicker);
				}

				cells.Add(buttonInstance);
			}
		}

		public void OnClick_CalendarBtn()
		{
			group.gameObject.SetActive(!group.gameObject.activeSelf);
		}
	}
}