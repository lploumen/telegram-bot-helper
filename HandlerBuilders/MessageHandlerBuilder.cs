using System;
using System.Collections.Generic;
using Telegram.Bot.Helper.HandlerBuilders.MessageHandlerBuilders;
using Telegram.Bot.Helper.Handlers;
using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.Helper.HandlerBuilders
{
    /// <summary>
    /// Builder for message updates
    /// </summary>
    /// <typeparam name="TLocalizationModel">Localization model</typeparam>
    public sealed class MessageHandlerBuilder<TLocalizationModel> where TLocalizationModel : class, new()
    {
        private readonly List<MessageHandler<TLocalizationModel>> _expressionList;
        
        internal MessageHandlerBuilder(List<MessageHandler<TLocalizationModel>> expressionList)
        {
            _expressionList = expressionList;
            
            Global = new MessageHandlerBuilderRule<TLocalizationModel>(expressionList, null);
            Private = new MessageHandlerBuilderRule<TLocalizationModel>(expressionList,
                t => t == ChatType.Private);
            Group = new MessageHandlerBuilderRule<TLocalizationModel>(expressionList,
                t => t == ChatType.Group);
            Supergroup = new MessageHandlerBuilderRule<TLocalizationModel>(expressionList,
                t => t == ChatType.Supergroup);
            Channel = new MessageHandlerBuilderRule<TLocalizationModel>(expressionList,
                t => t == ChatType.Channel);
        }
        
        public MessageHandlerBuilderRule<TLocalizationModel> Global { get; }
        public MessageHandlerBuilderRule<TLocalizationModel> Group { get; }
        public MessageHandlerBuilderRule<TLocalizationModel> Supergroup { get; }
        public MessageHandlerBuilderRule<TLocalizationModel> Private { get; }
        public MessageHandlerBuilderRule<TLocalizationModel> Channel { get; }

        public MessageHandlerBuilderRule<TLocalizationModel> this[Func<ChatType, bool> typePredicate] =>
            new MessageHandlerBuilderRule<TLocalizationModel>(_expressionList, typePredicate);
    }
}
