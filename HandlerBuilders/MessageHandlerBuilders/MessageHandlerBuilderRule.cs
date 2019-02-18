using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Helper.Handlers;
using Telegram.Bot.Helper.Handlers.MessageModels;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.Helper.HandlerBuilders.MessageHandlerBuilders
{
    public sealed class MessageHandlerBuilderRule<TLocalizationModel> where TLocalizationModel : class, new()
    {
        private readonly List<MessageHandler<TLocalizationModel>> _expressionList;
        private readonly Func<ChatType, bool> _typePredicate;

        internal MessageHandlerBuilderRule(
            List<MessageHandler<TLocalizationModel>> expressionList,
            Func<ChatType, bool> typePredicate)
        {
            _expressionList = expressionList;
            _typePredicate = typePredicate;
            
            Contains = new MessageTextPredicateHandlerBuilder<TLocalizationModel>(expressionList,
                (messageText, text, comparison) => messageText.IndexOf(text, comparison) != -1,
                typePredicate);
            StartsWith = new MessageTextPredicateHandlerBuilder<TLocalizationModel>(expressionList,
                (messageText, text, comparison) => messageText.StartsWith(text, comparison),
                typePredicate);
            EndsWith = new MessageTextPredicateHandlerBuilder<TLocalizationModel>(expressionList,
                (messageText, text, comparison) => messageText.EndsWith(text, comparison),
                typePredicate);
        }

        /// <summary>
        /// Handler for text messages containing any of specific texts
        /// </summary>
        public MessageTextPredicateHandlerBuilder<TLocalizationModel> Contains { get; }

        /// <summary>
        /// Handler for text messages starting with any of specific texts
        /// </summary>
        public MessageTextPredicateHandlerBuilder<TLocalizationModel> StartsWith { get; }

        /// <summary>
        /// Handler for text messages ending with any of specific texts
        /// </summary>
        public MessageTextPredicateHandlerBuilder<TLocalizationModel> EndsWith { get; }

        /// <summary>
        /// Called when message with photo type received
        /// </summary>
        public Func<PhotoMessage, TLocalizationModel, Task> OnPhotoAsync { get; set; }

        /// <summary>
        /// Called when message with video type received
        /// </summary>
        public Func<VideoMessage, TLocalizationModel, Task> OnVideoAsync { get; set; }

        /// <summary>
        /// Called when message with audio type received
        /// </summary>
        public Func<AudioMessage, TLocalizationModel, Task> OnAudioAsync { get; set; }

        /// <summary>
        /// Called when message with animation type received
        /// </summary>
        public Func<AnimationMessage, TLocalizationModel, Task> OnAnimationAsync { get; set; }

        /// <summary>
        /// Handler for text message
        /// </summary>
        /// <param name="text">Process if message text equals to specific value</param>
        /// <param name="verified">Restrict access to this handler for specific verify values only</param>
        /// <param name="comparison">Text comparison rule</param>
        /// <returns></returns>
        public Func<Message, TLocalizationModel, Task> this[string text, Verify verified = Verify.Unchecked, StringComparison comparison = StringComparison.Ordinal]
        {
            set
            {
                _expressionList.Add(new MessageHandler<TLocalizationModel>(
                    m => (_typePredicate == null || _typePredicate(m.Chat.Type)) && m.Type == MessageType.Text &&
                         m.Text.Equals(text, comparison), value, verified));
            }
        }

        /// <summary>
        /// Handlers for text message
        /// </summary>
        /// <param name="texts">Process if message text equals to specific value</param>
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