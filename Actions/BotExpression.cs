using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Telegram.Bot.Helper.Actions
{
    public class BotExpression
    {
        public readonly Func<Message, bool> Expression;
        public readonly Func<Message, Verify, Dictionary<string, string>, Task> Callback;
        public readonly Verify Verified;

        public BotExpression(Func<Message, bool> expression,
            Func<Message, Verify, Dictionary<string, string>, Task> callback,
            Verify verified)
        {
            Callback = callback;
            Expression = expression;
            Verified = verified;
        }

        public class Builder
        {
            private readonly List<BotExpression> _expressionList;

            public Builder(ref List<BotExpression> expressionList) => _expressionList = expressionList;
            
            public Func<Message, Verify, Dictionary<string, string>, Task> this[Func<Message, bool> expression,
                Verify verified = Verify.Unchecked]
            {
                set => _expressionList.Add(new BotExpression(expression, value, verified));
            }
        }
    }
}
