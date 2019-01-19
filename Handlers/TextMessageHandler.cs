using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Telegram.Bot.Helper.Handlers
{
    internal sealed class TextMessageHandler<TLocalizationModel> where TLocalizationModel : class, new()
    {
        internal readonly Func<TLocalizationModel, string> Message;
        internal readonly Func<Message, TLocalizationModel, Task> Callback;
        internal readonly Verify Verified;

        internal TextMessageHandler(Func<Message, TLocalizationModel, Task> callback, Func<TLocalizationModel, string> message, Verify verified)
        {
            Message = message;
            Callback = callback;
            Verified = verified;
        }
    }
}
