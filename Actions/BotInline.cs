using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Telegram.Bot.Helper.Actions
{
    public class BotInline
    {
        public readonly InlineCommand Command;
        public readonly Func<CallbackQueryInfo, string[], Verify, Dictionary<string, string>, Task> Callback;
        public readonly Verify Verified;

        public BotInline(Func<CallbackQueryInfo, string[], Verify, Dictionary<string, string>, Task> callback, string command, string separator, Verify verified)
        {
            Command = new InlineCommand(command, separator);
            Callback = callback;
            Verified = verified;
        }

        public class Builder
        {
            private readonly List<BotInline> _callbacks;
            private readonly string _separator;

            internal Builder(ref List<BotInline> callbacks, string separator)
            {
                _callbacks = callbacks;
            }

            public Func<CallbackQueryInfo, string[], Verify, Dictionary<string, string>, Task> this[string command, Verify verified = Verify.Unchecked]
            {
                set => _callbacks.Add(new BotInline(value, command, _separator, verified));
            }
        }
    }
}
