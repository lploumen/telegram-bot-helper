using Telegram.Bot.Helper.Widgets.Settings;

namespace Telegram.Bot.Helper
{
    public class WidgetSettings<TLocalizationModel> where TLocalizationModel : class, new()
    {
        public CalendarSettings<TLocalizationModel> Calendar;
    }
}
