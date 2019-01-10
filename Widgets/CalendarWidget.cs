using System;
using System.Collections.Generic;
using System.Globalization;
using Telegram.Bot.Helper.Widgets.Data;
using Telegram.Bot.Helper.Widgets.Settings;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Helper.Widgets
{
    internal class CalendarWidget<TLocalizationModel> : InlineKeyboardMarkup where TLocalizationModel : class, new()
    {
        internal CalendarWidget(CalendarSettings<TLocalizationModel> calendarSettings, CalendarData calendarData, char separator, TLocalizationModel localizationModel)
            : base(GetCalendarKeyboard(calendarSettings, calendarData, separator, localizationModel)) { }

        private static List<List<InlineKeyboardButton>> GetCalendarKeyboard(CalendarSettings<TLocalizationModel> calendarSettings, CalendarData calendarData,
            char separator, TLocalizationModel localizationModel)
        {
            var separatorStr = separator.ToString();
            var formatter = new CultureInfo(calendarSettings.CultureInfoName(localizationModel)).DateTimeFormat;

            var minDate = calendarData.MinAllowedDate;
            var maxDate = calendarData.MaxAllowedDate;
            var date = calendarData.CurrentPosition;

            var keyboard = new List<List<InlineKeyboardButton>>
            {
                new List<InlineKeyboardButton>
                {
                    new InlineKeyboardButton
                    {
                        Text = $"{formatter.GetMonthName(calendarData.CurrentPosition.Month)} {date.Year}",
                        CallbackData = "ignore"
                    }
                }
            };

            var days = new List<InlineKeyboardButton>();
            for (var i = calendarSettings.StartsFromMonday ? 1 : 0; i < 7; i++)
                days.Add(new InlineKeyboardButton { Text = formatter.GetShortestDayName((DayOfWeek)i), CallbackData = "ignore" });
            if (calendarSettings.StartsFromMonday)
                days.Add(new InlineKeyboardButton { Text = formatter.GetShortestDayName(DayOfWeek.Sunday), CallbackData = "ignore" });
            keyboard.Add(days);

            var numbers = new List<List<InlineKeyboardButton>>
            {
                new List<InlineKeyboardButton>()
            };
            var daysInMonth = DateTime.DaysInMonth(calendarData.CurrentPosition.Year, calendarData.CurrentPosition.Month);
            var dt = new DateTime(calendarData.CurrentPosition.Year, calendarData.CurrentPosition.Month, 1);

            for (var day = 1; day <= daysInMonth; day++)
            {
                var ignore = false;
                var empty = false;

                if (minDate != null)
                {
                    var (Year, Month, Day) = minDate.Value;
                    if (Year == date.Year && Month == date.Month && Day > day)
                        empty = true;
                }

                if (!empty && maxDate != null)
                {
                    var (Year, Month, Day) = maxDate.Value;
                    if (Year == date.Year && Month == date.Month && Day < day)
                        empty = true;
                }

                if (!empty && !ignore && calendarSettings.IgnoreDaysOfWeek != null)
                    foreach (var dayOfWeekToIgnore in calendarSettings.IgnoreDaysOfWeek)
                        if (dt.DayOfWeek == dayOfWeekToIgnore)
                        {
                            ignore = true;
                            break;
                        }

                if (!empty && !ignore && calendarSettings.IgnoreDates != null)
                    foreach (var (Year, Month, Day) in calendarSettings.IgnoreDates)
                        if (date.Year == Year && date.Month == Month && day == Day)
                        {
                            ignore = true;
                            break;
                        }
                
                numbers[numbers.Count - 1].Add(new InlineKeyboardButton
                {
                    Text = empty ? " " : (!ignore || calendarSettings.IgnoreMode == CalendarSettings<TLocalizationModel>.IgnoreModeItem.ShowDayOnButton)
                        ? day.ToString() : calendarSettings.IgnoreMode == CalendarSettings<TLocalizationModel>.IgnoreModeItem.UsePlaceholder
                            ? (calendarSettings.IgnorePlaceholder(localizationModel) ?? " ") : " ",

                    CallbackData = ignore || empty ? "ignore"
                        : string.Join(separatorStr, "widget", "calendar", calendarData.CurrentPosition.Year, calendarData.CurrentPosition.Month, day)
                });

                if (((calendarSettings.StartsFromMonday && dt.DayOfWeek == DayOfWeek.Sunday)
                    || (!calendarSettings.StartsFromMonday && dt.DayOfWeek == DayOfWeek.Saturday))
                    && day != daysInMonth)
                    numbers.Add(new List<InlineKeyboardButton>());

                dt = dt.AddDays(1d);
            }

            int count = 7 - numbers[0].Count;
            for (int i = 0; i < count; i++)
                numbers[0].Insert(0, new InlineKeyboardButton { Text = " ", CallbackData = "ignore" });
            count = 7 - numbers[numbers.Count - 1].Count;
            for (int i = 0; i < count; i++)
                numbers[numbers.Count - 1].Add(new InlineKeyboardButton { Text = " ", CallbackData = "ignore" });
            keyboard.AddRange(numbers);

            var changeMonthRow = new List<InlineKeyboardButton>();
            var previousMonthEnabled = minDate == null || date.Year > minDate.Value.Year ||
                (date.Year == minDate.Value.Year && date.Month > minDate.Value.Month);
            var nextMonthEnabled = maxDate == null || date.Year < maxDate.Value.Year ||
                (date.Year == maxDate.Value.Year && date.Month < maxDate.Value.Month);
            changeMonthRow.Add(new InlineKeyboardButton { Text = calendarSettings.MonthText(localizationModel), CallbackData = "ignore" });
            changeMonthRow.Add(new InlineKeyboardButton
            {
                Text = nextMonthEnabled ? calendarSettings.NextMonthText(localizationModel) : " ",
                CallbackData = nextMonthEnabled ? string.Join(separatorStr, "widget", "calendar", dt.Year, dt.Month) : "ignore"
            });
            dt = dt.AddMonths(-2);
            changeMonthRow.Insert(0, new InlineKeyboardButton
            {
                Text = previousMonthEnabled ? calendarSettings.PreviousMonthText(localizationModel) : " ",
                CallbackData = previousMonthEnabled ? string.Join(separatorStr, "widget", "calendar", dt.Year, dt.Month) : "ignore"
            });
            keyboard.Add(changeMonthRow);

            var changeYearRow = new List<InlineKeyboardButton>();
            var previousYearEnabled = minDate == null || date.Year > minDate.Value.Year + 1 ||
                (date.Year == minDate.Value.Year + 1 && date.Month >= minDate.Value.Month);
            var nextYearEnabled = maxDate == null || date.Year < maxDate.Value.Year - 1 ||
                (date.Year == maxDate.Value.Year - 1 && date.Month <= maxDate.Value.Month);
            changeYearRow.Add(new InlineKeyboardButton
            {
                Text = previousYearEnabled ? calendarSettings.PreviousYearText(localizationModel) : " ",
                CallbackData = previousYearEnabled ? string.Join(separatorStr, "widget", "calendar", date.Year - 1, date.Month) : "ignore"
            });
            changeYearRow.Add(new InlineKeyboardButton { Text = calendarSettings.YearText(localizationModel), CallbackData = "ignore" });
            changeYearRow.Add(new InlineKeyboardButton
            {
                Text = nextYearEnabled ? calendarSettings.NextYearText(localizationModel) : " ",
                CallbackData = nextYearEnabled ? string.Join(separatorStr, "widget", "calendar", date.Year + 1, date.Month) : "ignore"
            });
            keyboard.Add(changeYearRow);

            return keyboard;
        }
    }
}
