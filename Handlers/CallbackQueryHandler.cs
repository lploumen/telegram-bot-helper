using System;
using System.Threading.Tasks;

namespace Telegram.Bot.Helper.Handlers
{
    internal sealed class CallbackQueryHandler<TLocalizationModel> where TLocalizationModel : class, new()
    {
        internal readonly CallbackQueryCommand Command;
        internal readonly Func<CallbackQueryInfo, string[], TLocalizationModel, Task> Callback;
        internal readonly Verify Verified;

        internal CallbackQueryHandler(in Func<CallbackQueryInfo, string[], TLocalizationModel, Task> callback, in string data, in char separator, in Verify verified)
        {
            Command = new CallbackQueryCommand(data, separator);
            Callback = callback;
            Verified = verified;
        }
    }
}
