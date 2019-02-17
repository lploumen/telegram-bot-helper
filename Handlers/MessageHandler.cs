using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Telegram.Bot.Helper.Handlers
{
    internal sealed class MessageHandler<TLocalizationModel> where TLocalizationModel : class, new()
    {
        internal readonly Func<Message, bool> Predicate;
        internal readonly Func<Message, TLocalizationModel, Task> Callback;
        internal readonly Verify Verified;

        internal MessageHandler(Func<Message, bool> predicate,
            Func<Message, TLocalizationModel, Task> callback,
            Verify verified)
        {
            Callback = callback;
            Predicate = predicate;
            Verified = verified;
        }
    }
}
