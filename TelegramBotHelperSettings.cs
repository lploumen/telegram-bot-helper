using Telegram.Bot.Helper.Localization;

namespace Telegram.Bot.Helper
{
    /// <summary>
    /// Settings for <see cref="TelegramBotHelper{TLocalizationModel}"/> class
    /// </summary>
    public sealed class TelegramBotHelperSettings
    {
        /// <summary>
        /// Localization settings. This field is always not null.
        /// </summary>
        public readonly LocalizationSettings Localization = new LocalizationSettings();

        /// <summary>
        /// If true, bot will ignore messages from private chats, groups and supergroups. Defaults to false.
        /// </summary>
        public bool IgnoreMessages;

        /// <summary>
        /// If true, bot will ignore messages that were edited by users in private chats, groups and supergroups. Defaults to false.
        /// </summary>
        public bool IgnoreEditedMessages;

        /// <summary>
        /// If true, bot will ignore messages that were sent to the channel. Defaults to false.
        /// </summary>
        public bool IgnoreChannelPosts;

        /// <summary>
        /// If true, bot will ignore messages that were edited by users in the channels. Defaults to false.
        /// </summary>
        public bool IgnoreEditedChannelPosts;
    }
}
