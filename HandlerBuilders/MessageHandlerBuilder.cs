﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Helper.Handlers;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.Helper.HandlerBuilders
{
    /// <summary>
    /// Builder for message expressions
    /// </summary>
    /// <typeparam name="TLocalizationModel">Localization model</typeparam>
    public class MessageHandlerBuilder<TLocalizationModel> where TLocalizationModel : class, new()
    {
        private readonly List<MessageExpressionHandler<TLocalizationModel>> _expressionList;

        internal MessageHandlerBuilder(List<MessageExpressionHandler<TLocalizationModel>> expressionList) => _expressionList = expressionList;

        /// <summary>
        /// Handler for text message
        /// </summary>
        /// <param name="text">Process if message text equals to specified value</param>
        /// <param name="verified">Restrict access to this handler for specified verify values only</param>
        /// <param name="comparison">Text comparison rule</param>
        /// <returns></returns>
        public Func<Message, TLocalizationModel, Task> this[string text, Verify verified = Verify.Unchecked, StringComparison comparison = StringComparison.Ordinal]
        {
            set
            {
                _expressionList.Add(new MessageExpressionHandler<TLocalizationModel>(m => m.Type == MessageType.Text && m.Text == text, value, verified));
            }
        }

        /// <summary>
        /// Handlers for text message
        /// </summary>
        /// <param name="texts">Process if message text equals to specified value</param>
        /// <param name="verified">Restrict access to these handlers for specified verify values only</param>
        /// <param name="comparison">Text comparison rule</param>
        /// <returns></returns>
        public Func<Message, TLocalizationModel, Task> this[string[] texts, Verify verified = Verify.Unchecked, StringComparison comparison = StringComparison.Ordinal]
        {
            set
            {
                foreach (var text in texts)
                    this[text, verified, comparison] = value;
            }
        }
    }
}