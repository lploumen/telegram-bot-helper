using System;
using System.Threading.Tasks;

namespace Telegram.Bot.Helper.Handlers
{
    /// <summary>
    /// Callback query handler
    /// </summary>
    public class CallbackQueryHandler<TLocalizationModel> where TLocalizationModel : class, new()
    {
        public readonly CallbackQueryCommand Command;
        public readonly Func<CallbackQueryInfo, string[], Verify, TLocalizationModel, Task> Callback;
        public readonly Verify Verified;

        internal CallbackQueryHandler(Func<CallbackQueryInfo, string[], Verify, TLocalizationModel, Task> callback, string data, char separator, Verify verified)
        {
            Command = new CallbackQueryCommand(data, separator);
            Callback = callback;
            Verified = verified;
        }
    }
}
