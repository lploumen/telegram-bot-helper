using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Helper.Handlers;
using Telegram.Bot.Types;

namespace Telegram.Bot.Helper.HandlerBuilders
{
    public class MessageExpressionHandlerBuilder<TLocalizationModel> where TLocalizationModel : class, new()
    {
        private readonly List<MessageExpressionHandler<TLocalizationModel>> _expressionList;

        internal MessageExpressionHandlerBuilder(List<MessageExpressionHandler<TLocalizationModel>> expressionList) => _expressionList = expressionList;

        /// <param name="expression">Func that takes Message and returns bool</param>
        /// <param name="verified">Access restrictions</param>
        public Func<Message, Verify, TLocalizationModel, Task> this[Func<Message, bool> expression,
            Verify verified = Verify.Unchecked]
        {
            set => _expressionList.Add(new MessageExpressionHandler<TLocalizationModel>(expression, value, verified));
        }
    }
}
