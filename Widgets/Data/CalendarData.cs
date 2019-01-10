namespace Telegram.Bot.Helper.Widgets.Data
{
    public class CalendarData
    {
        /// <summary>
        /// Current position of calendar on first send. Must be between MinimalAllowedDate and MaximalAllowedDate.
        /// </summary>
        public (int Year, int Month) CurrentPosition;

        internal (int Year, int Month, int Day)? MinAllowedDate;
        
        internal (int Year, int Month, int Day)? MaxAllowedDate;
    }
}
