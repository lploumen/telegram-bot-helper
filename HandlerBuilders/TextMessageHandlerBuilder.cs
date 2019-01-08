using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Telegram.Bot.Helper.Handlers;
using Telegram.Bot.Types;

namespace Telegram.Bot.Helper.HandlerBuilders
{
    public class TextMessageHandlerBuilder<TLocalizationModel> where TLocalizationModel : class, new()
    {
        private readonly List<TextMessageHandler<TLocalizationModel>> _messages;

        internal TextMessageHandlerBuilder(List<TextMessageHandler<TLocalizationModel>> messages) => _messages = messages;

        public Func<Message, Verify, TLocalizationModel, Task> this[Expression<Func<TLocalizationModel, string>> message, Verify verified = Verify.Unchecked]
        {
            set
            {
                _messages.Add(new TextMessageHandler<TLocalizationModel>(value, message, verified));
            }
        }

        public Func<Message, Verify, TLocalizationModel, Task> this[string message, Verify verified = Verify.Unchecked]
        {
            set
            {
                Expression<Func<TLocalizationModel, string>> expression = _ => message;
                _messages.Add(new TextMessageHandler<TLocalizationModel>(value, expression, verified));
            }
        }

        public Func<Message, Verify, TLocalizationModel, Task> this[IEnumerable<Expression<Func<TLocalizationModel, string>>> messages, Verify verified = Verify.Unchecked]
        {
            set
            {
                foreach (var message in messages)
                    _messages.Add(new TextMessageHandler<TLocalizationModel>(value, message, verified));
            }
        }

        public Func<Message, Verify, TLocalizationModel, Task> this[IEnumerable<string> messages, Verify verified = Verify.Unchecked]
        {
            set
            {
                foreach (var message in messages)
                {
                    Expression<Func<TLocalizationModel, string>> expression = _ => message;
                    _messages.Add(new TextMessageHandler<TLocalizationModel>(value, expression, verified));
                }
            }
        }
    }
}
