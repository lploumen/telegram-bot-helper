using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Helper.Widgets.Data;
using Telegram.Bot.Types;

namespace Telegram.Bot.Helper.Widgets.Settings
{
    /// <summary>
    /// Settings for calendar widget
    /// </summary>
    /// <typeparam name="TLocalizationModel">Localization model class</typeparam>
    public class CalendarSettings<TLocalizationModel> where TLocalizationModel : class, new()
    {
        /// <summary>
        /// Minimal allowed date that can be selected by user. Not null.
        /// </summary>
        public Func<User, Task<(int Year, int Month, int Day)?>> MinAllowedDate;

        /// <summary>
        /// Maximal allowed date that can be selected by user. Not null.
        /// </summary>
        public Func<User, Task<(int Year, int Month, int Day)?>> MaxAllowedDate;

        /// <summary>
        /// Text on button which is placed between previous / next month switches. Not null.
        /// </summary>
        public Func<TLocalizationModel, string> MonthText;

        /// <summary>
        /// Text on button which will move calendar to previous month. Not null.
        /// </summary>
        public Func<TLocalizationModel, string> PreviousMonthText;

        /// <summary>
        /// Text on button which will move calendar to next month. Not null.
        /// </summary>
        public Func<TLocalizationModel, string> NextMonthText;

        /// <summary>
        /// Text on button which is placed between previous / next year switches. Not null.
        /// </summary>
        public Func<TLocalizationModel, string> YearText;

        /// <summary>
        /// Text on button which will move calendar to previous year. Not null.
        /// </summary>
        public Func<TLocalizationModel, string> PreviousYearText;

        /// <summary>
        /// Text on button which will move calendar to next year. Not null.
        /// </summary>
        public Func<TLocalizationModel, string> NextYearText;

        /// <summary>
        /// Delegate that will be called when user selects the date. Not null.
        /// </summary>
        public Func<(int Year, int Month, int Day), User, Verify, TLocalizationModel, Task> DateSelected;

        /// <summary>
        /// Specify which days of week will be ignored on selection. Can be null.
        /// </summary>
        public IEnumerable<DayOfWeek> IgnoreDaysOfWeek;

        /// <summary>
        /// Placeholder text for ignored days (to replace empty strings). Not null if <see cref="IgnoreMode"/>'s value is <see cref="IgnoreModeItem.UsePlaceholder"/>
        /// </summary>
        public Func<TLocalizationModel, string> IgnorePlaceholder;

        /// <summary>
        /// Ignore mode for dates specified as ignored. For every mode, click on button will be ignored.
        /// </summary>
        public IgnoreModeItem IgnoreMode = IgnoreModeItem.NoTextOnButton;

        /// <summary>
        /// Mode for dates specified as ignored.
        /// </summary>
        public enum IgnoreModeItem
        {
            NoTextOnButton,
            ShowDayOnButton,
            UsePlaceholder
        }

        /// <summary>
        /// CultureInfo name (ru, en, de, ...) instance used for date formatting. Not null.
        /// </summary>
        public Func<TLocalizationModel, string> CultureInfoName;

        /// <summary>
        /// If true, calendar will start from monday; otherwise from sunday. Defaults to false.
        /// </summary>
        public bool StartsFromMonday;

        /// <summary>
        /// Enumerable of dates that must be ignored by calendar
        /// </summary>
        public IEnumerable<(int Year, int Month, int Day)> IgnoreDates;

        internal void EnsureSettingsAreCorrect()
        {
            if (IgnoreMode == IgnoreModeItem.UsePlaceholder && IgnorePlaceholder == null)
                throw new ArgumentNullException(nameof(IgnorePlaceholder));
            if (PreviousYearText == null)
                throw new ArgumentNullException(nameof(PreviousYearText));
            if (NextYearText == null)
                throw new ArgumentNullException(nameof(NextYearText));
            if (YearText == null)
                throw new ArgumentNullException(nameof(YearText));
            if (PreviousMonthText == null)
                throw new ArgumentNullException(nameof(PreviousMonthText));
            if (NextMonthText == null)
                throw new ArgumentNullException(nameof(NextMonthText));
            if (MonthText == null)
                throw new ArgumentNullException(nameof(MonthText));
            if (CultureInfoName == null)
                throw new ArgumentNullException(nameof(CultureInfoName));
            if (DateSelected == null)
                throw new ArgumentNullException(nameof(DateSelected));
            if (MinAllowedDate == null)
                throw new ArgumentNullException(nameof(MinAllowedDate));
            if (MaxAllowedDate == null)
                throw new ArgumentNullException(nameof(MaxAllowedDate));
        }

        internal static void EnsureMinAllowedDateIsCorrect((int Year, int Month, int Day) minDate, (int Year, int Month) currentPosition)
        {
            if (minDate.Year > currentPosition.Year || (minDate.Year == currentPosition.Year && minDate.Month > currentPosition.Month))
                throw new ArgumentException($"{nameof(CalendarData.CurrentPosition)} must equal to or be greater than {nameof(MinAllowedDate)}.");
        }

        internal static void EnsureMaxAllowedDateIsCorrect((int Year, int Month, int Day) maxDate, (int Year, int Month) currentPosition)
        {
            if (maxDate.Year < currentPosition.Year || (maxDate.Year == currentPosition.Year && maxDate.Month < currentPosition.Month))
                throw new ArgumentException($"{nameof(CalendarData.CurrentPosition)} must equal to or be less than {nameof(MaxAllowedDate)}.");
        }
    }
}
