namespace Telegram.Bot.Helper.Localization
{
    /// <summary>
    /// Settings for localization
    /// </summary>
    public sealed class LocalizationSettings
    {
        /// <summary>
        /// IETF code of default language (en, ru, de, ...). Required. Defaults to 'en'.
        /// </summary>
        public string DefaultLocalizationKey = "en";
    }
}
