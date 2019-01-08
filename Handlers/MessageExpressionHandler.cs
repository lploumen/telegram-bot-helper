using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Telegram.Bot.Helper.Handlers
{
    public class MessageExpressionHandler<TLocalizationModel> where TLocalizationModel : class, new()
    {
        public readonly Func<Message, bool> Expression;
        public readonly Func<Message, Verify, TLocalizationModel, Task> Callback;
        public readonly Verify Verified;

        internal MessageExpressionHandler(Func<Message, bool> expression,
            Func<Message, Verify, TLocalizationModel, Task> callback,
            Verify verified)
        {
            Callback = callback;
            Expression = expression;
            Verified = verified;
        }
        
        public class Builder
        {
            private readonly List<MessageExpressionHandler<TLocalizationModel>> _expressionList;

            internal Builder(List<MessageExpressionHandler<TLocalizationModel>> expressionList) => _expressionList = expressionList;
            
            /// <param name="expression">Func that takes Message and returns bool</param>
            /// <param name="verified">Access restrictions</param>
            public Func<Message, Verify, TLocalizationModel, Task> this[Func<Message, bool> expression,
                Verify verified = Verify.Unchecked]
            {
                set => _expressionList.Add(new MessageExpressionHandler<TLocalizationModel>(expression, value, verified));
            }
        }
    }
}
