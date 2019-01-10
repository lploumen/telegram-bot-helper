using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Telegram.Bot.Helper.Handlers
{
    public class TextMessageHandler<TLocalizationModel> where TLocalizationModel : class, new()
    {
        public readonly Func<TLocalizationModel, string> Message;
        public readonly Func<Message, Verify, TLocalizationModel, Task> Callback;
        public readonly Verify Verified;

        internal TextMessageHandler(Func<Message, Verify, TLocalizationModel, Task> callback, Func<TLocalizationModel, string> message, Verify verified)
        {
            Message = message;
            Callback = callback;
            Verified = verified;
        }
    }
}
