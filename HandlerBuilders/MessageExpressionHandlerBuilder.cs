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
        
        public Func<Message, Verify, TLocalizationModel, Task> this[Func<Message, bool> expression, Verify verified = Verify.Unchecked]
        {
            set
            {
                if (expression == null)
                    throw new ArgumentNullException(nameof(expression));
                if (value == null)
                    throw new ArgumentNullException("value");

                _expressionList.Add(new MessageExpressionHandler<TLocalizationModel>(expression, value, verified));
            }
        }
    }
}
