using System;

namespace Telegram.Bot.Helper
{
    /// <summary>
    /// Callback query command is callback data of inline button.
    /// </summary>
    public class CallbackQueryCommand
    {
        /// <summary>
        /// Count of commands
        /// </summary>
        public int Count => Commands.Length;

        /// <summary>
        /// Commands separated from callback data
        /// </summary>
        public readonly string[] Commands;

        internal CallbackQueryCommand(string command, char separator)
        {
            Commands = command.Split(new[] { separator }, StringSplitOptions.None);
        }

        internal bool Equals(CallbackQueryCommand valueToCompareWith)
        {
            if (valueToCompareWith == null || Count != valueToCompareWith.Count)
                return false;

            for (int commandIndex = 0; commandIndex < Count; commandIndex++)
            {
                if (string.IsNullOrWhiteSpace(Commands[commandIndex]))
                    continue;

                if (Commands[commandIndex] != valueToCompareWith.Commands[commandIndex])
                    return false;
            }
            return true;
        }
    }
}
