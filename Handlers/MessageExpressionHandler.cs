using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Telegram.Bot.Helper.Handlers
{
    internal sealed class MessageExpressionHandler<TLocalizationModel> where TLocalizationModel : class, new()
    {
        internal readonly Func<Message, bool> Expression;
        internal readonly Func<Message, TLocalizationModel, Task> Callback;
        internal readonly Verify Verified;

        internal MessageExpressionHandler(Func<Message, bool> expression,
            Func<Message, TLocalizationModel, Task> callback,
            Verify verified)
        {
            Callback = callback;
            Expression = expression;
            Verified = verified;
        }
    }
}
