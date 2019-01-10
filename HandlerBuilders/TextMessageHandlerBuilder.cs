using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Helper.Handlers;
using Telegram.Bot.Types;

namespace Telegram.Bot.Helper.HandlerBuilders
{
    public class TextMessageHandlerBuilder<TLocalizationModel> where TLocalizationModel : class, new()
    {
        private readonly List<TextMessageHandler<TLocalizationModel>> _messages;

        internal TextMessageHandlerBuilder(List<TextMessageHandler<TLocalizationModel>> messages) => _messages = messages;

        public Func<Message, Verify, TLocalizationModel, Task> this[Func<TLocalizationModel, string> message, Verify verified = Verify.Unchecked]
        {
            set
            {
                if (message == null)
                    throw new ArgumentNullException(nameof(message));
                if (value == null)
                    throw new ArgumentNullException("value");

                _messages.Add(new TextMessageHandler<TLocalizationModel>(value, message, verified));
            }
        }

        public Func<Message, Verify, TLocalizationModel, Task> this[string message, Verify verified = Verify.Unchecked]
        {
            set
            {
                if (message == null)
                    throw new ArgumentNullException(nameof(message));
                if (value == null)
                    throw new ArgumentNullException("value");

                _messages.Add(new TextMessageHandler<TLocalizationModel>(value, _ => message, verified));
            }
        }

        public Func<Message, Verify, TLocalizationModel, Task> this[IEnumerable<Func<TLocalizationModel, string>> messages, Verify verified = Verify.Unchecked]
        {
            set
            {
                if (messages == null)
                    throw new ArgumentNullException(nameof(messages));
                if (value == null)
                    throw new ArgumentNullException("value");

                int index = 0;
                foreach (var message in messages)
                {
                    if (message == null)
                        throw new ArgumentNullException($"{nameof(messages)}[{index}]");

                    _messages.Add(new TextMessageHandler<TLocalizationModel>(value, message, verified));
                    ++index;
                }
            }
        }

        public Func<Message, Verify, TLocalizationModel, Task> this[IEnumerable<string> messages, Verify verified = Verify.Unchecked]
        {
            set
            {
                if (messages == null)
                    throw new ArgumentNullException(nameof(messages));
                if (value == null)
                    throw new ArgumentNullException("value");

                int index = 0;
                foreach (var message in messages)
                {
                    if (message == null)
                        throw new ArgumentNullException($"{nameof(messages)}[{index}]");

                    _messages.Add(new TextMessageHandler<TLocalizationModel>(value, _ => message, verified));
                    ++index;
                }
            }
        }
    }
}
