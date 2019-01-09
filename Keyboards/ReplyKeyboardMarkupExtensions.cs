using System;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Helper.Keyboards
{
    public static class ReplyKeyboardMarkupExtensions
    {
        /// <summary>
        /// Add button with specified text to the keyboard.
        /// </summary>
        /// <param name="text">Text on button. Not null.</param>
        public static Builder<KeyboardButton> Text(this Builder<KeyboardButton> builder, string text)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            builder.Keyboard[builder.Index].Add(new KeyboardButton(text));
            return builder;
        }

        /// <summary>
        /// Add button which will request user's contact to the keyboard.
        /// </summary>
        /// <param name="text">Text on button. Not null.</param>
        public static Builder<KeyboardButton> Contact(this Builder<KeyboardButton> builder, string text)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            builder.Keyboard[builder.Index].Add(new KeyboardButton(text) { RequestContact = true });
            return builder;
        }

        /// <summary>
        /// Add button which will request user's current location to the keyboard.
        /// </summary>
        /// <param name="text">Text on button. Not null.</param>
        public static Builder<KeyboardButton> Location(this Builder<KeyboardButton> builder, string text)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            builder.Keyboard[builder.Index].Add(new KeyboardButton(text) { RequestLocation = true });
            return builder;
        }

        /// <summary>
        /// Build markup from keyboard
        /// </summary>
        public static ReplyKeyboardMarkup M(this IEnumerable<IEnumerable<KeyboardButton>> keyboard) =>
            new ReplyKeyboardMarkup(keyboard);

        /// <summary>
        /// Build markup from keyboard
        /// </summary>
        public static ReplyKeyboardMarkup M(this IEnumerable<KeyboardButton> keyboard) =>
            new ReplyKeyboardMarkup(keyboard);

        /// <summary>
        /// Builds markup from keyboard
        /// </summary>
        public static ReplyKeyboardMarkup M(this KeyboardButton keyboard) =>
            new ReplyKeyboardMarkup(keyboard);

        /// <summary>
        /// Build markup from keyboard
        /// </summary>
        public static ReplyKeyboardMarkup M(this Builder<KeyboardButton> builder) =>
            new ReplyKeyboardMarkup(builder.Keyboard);

        /// <summary>
        /// Set ResizeKeyboard property value
        /// </summary>
        /// <param name="value">New value</param>
        public static ReplyKeyboardMarkup RK(this ReplyKeyboardMarkup markup, bool value = true)
        {
            markup.ResizeKeyboard = value;
            return markup;
        }

        /// <summary>
        /// Set OneTimeKeyboard property value
        /// </summary>
        /// <param name="value">New value</param>
        public static ReplyKeyboardMarkup OTK(this ReplyKeyboardMarkup markup, bool value = true)
        {
            markup.OneTimeKeyboard = value;
            return markup;
        }
    }
}
