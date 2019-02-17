using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Helper.Handlers;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.Helper.HandlerBuilders.MessageHandlerBuilders
{
    public sealed class MessageTextPredicateHandlerBuilder<TLocalizationModel> where TLocalizationModel : class, new()
    {
        private readonly List<MessageExpressionHandler<TLocalizationModel>> _expressionList;
        private readonly Func<MessageType, bool> _typePredicate;
        private readonly Func<string, string, StringComparison, bool> _textPredicate;

        internal MessageTextPredicateHandlerBuilder(List<MessageExpressionHandler<TLocalizationModel>> expressionList,
            Func<string, string, StringComparison, bool> textPredicate, Func<MessageType, bool> typePredicate)
        {
            _expressionList = expressionList;
            _typePredicate = typePredicate;
            _textPredicate = textPredicate;
        }

        /// <summary>
        /// Handler for text message containing of specific text
        /// </summary>
        /// <param name="text">Process if message text contains specific value</param>
        /// <param name="verified">Restrict access to this handler for specific verify statuses only</param>
        /// <param name="comparison">Text comparison rule</param>
        /// <returns></returns>
        public Func<Message, TLocalizationModel, Task> this[string text, Verify verified = Verify.Unchecked, StringComparison comparison = StringComparison.Ordinal]
        {
            set
            {
                _expressionList.Add(new MessageExpressionHandler<TLocalizationModel>(m =>
                {
                    if (_typePredicate != null && !_typePredicate(m.Type))
                        return false;
                    
                    return m.Type == MessageType.Text && _textPredicate(m.Text, text, comparison);
                }, value, verified));
            }
        }
        
        /// <summary>
        /// Handler for text message containing any of specific texts
        /// </summary>
        /// <param name="texts">Process if message text contains specific values</param>
        /// <param name="verified">Restrict access to these handlers for specific verify values only</param>
        /// <param name="comparison">Text comparison rule</param>
        /// <returns></returns>
        public Func<Message, TLocalizationModel, Task> this[IEnumerable<string> texts, Verify verified = Verify.Unchecked, StringComparison comparison = StringComparison.Ordinal]
        {
            set
            {
                foreach (var text in texts)
                    this[text, verified, comparison] = value;
            }
        }
    }
}