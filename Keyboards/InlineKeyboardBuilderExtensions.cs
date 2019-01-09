using System;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Helper.Keyboards
{
    /// <summary>
    /// Extensions for building inline keyboard ( https://core.telegram.org/bots/api#inlinekeyboardbutton ).
    /// </summary>
    public static class InlineKeyboardBuilderExtensions
    {
        /// <summary>
        /// Add inline button with url to the keyboard.
        /// </summary>
        /// <param name="text">Text on button. Not null.</param>
        /// <param name="url">Url ( http / https resource or tg://user?id=user_id ). Not null.</param>
        public static Builder<InlineKeyboardButton> Url(this Builder<InlineKeyboardButton> builder, string text, string url)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            builder.Keyboard[builder.Index].Add(new InlineKeyboardButton { Url = url, Text = text });
            return builder;
        }

        /// <summary>
        /// Add inline button with callback data to the keyboard.
        /// </summary>
        /// <param name="text">Text on button. Not null.</param>
        /// <param name="callbackData">Callback data. Not null.</param>
        public static Builder<InlineKeyboardButton> Data(this Builder<InlineKeyboardButton> builder, string text, string callbackData)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (callbackData == null)
                throw new ArgumentNullException(nameof(callbackData));

            builder.Keyboard[builder.Index].Add(new InlineKeyboardButton { CallbackData = callbackData, Text = text });
            return builder;
        }

        /// <summary>
        /// Add 'pay' button to the keyboard ( https://core.telegram.org/bots/api#payments ).
        /// </summary>
        /// <param name="text">Text on 'pay' button. Not null.</param>
        public static Builder<InlineKeyboardButton> Pay(this Builder<InlineKeyboardButton> builder, string text)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            builder.Keyboard[builder.Index].Add(new InlineKeyboardButton { Pay = true, Text = text });
            return builder;
        }

        /// <summary>
        /// Add inline button with SwitchInlineQuery parameter.
        /// </summary>
        /// <param name="text">Text on 'pay' button. Not null.</param>
        /// <param name="switchText">Inline query text to be inserted with bot's username in chat selected by user. Not null, but can be empty.</param>
        public static Builder<InlineKeyboardButton> Switch(this Builder<InlineKeyboardButton> builder, string text, string switchText)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (switchText == null)
                throw new ArgumentNullException(nameof(switchText));

            builder.Keyboard[builder.Index].Add(new InlineKeyboardButton { Text = text, SwitchInlineQuery = switchText });
            return builder;
        }

        /// <summary>
        /// Add inline button with SwitchInlineQueryCurrentChat parameter.
        /// </summary>
        /// <param name="text">Text on 'pay' button. Not null.</param>
        /// <param name="switchText">Inline query text to be inserted with bot's username in current chat's input field. Not null, but can be empty.</param>
        public static Builder<InlineKeyboardButton> SwitchCurrent(this Builder<InlineKeyboardButton> builder, string text, string switchText)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (switchText == null)
                throw new ArgumentNullException(nameof(switchText));

            builder.Keyboard[builder.Index].Add(new InlineKeyboardButton { Text = text, SwitchInlineQueryCurrentChat = switchText });
            return builder;
        }

        /// <summary>
        /// Build markup from keyboard
        /// </summary>
        public static InlineKeyboardMarkup M(this IEnumerable<IEnumerable<InlineKeyboardButton>> keyboard) =>
            new InlineKeyboardMarkup(keyboard);

        /// <summary>
        /// Build markup from keyboard
        /// </summary>
        public static InlineKeyboardMarkup M(this IEnumerable<InlineKeyboardButton> keyboard) =>
            new InlineKeyboardMarkup(keyboard);

        /// <summary>
        /// Build markup from keyboard
        /// </summary>
        public static InlineKeyboardMarkup M(this InlineKeyboardButton keyboard) =>
            new InlineKeyboardMarkup(keyboard);

        /// <summary>
        /// Build markup from keyboard
        /// </summary>
        public static InlineKeyboardMarkup M(this Builder<InlineKeyboardButton> builder) =>
            new InlineKeyboardMarkup(builder.Keyboard);
    }
}
