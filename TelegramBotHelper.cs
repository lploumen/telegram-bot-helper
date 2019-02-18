using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Telegram.Bot.Helper.HandlerBuilders;
using Telegram.Bot.Helper.Handlers;
using Telegram.Bot.Helper.Localization;
using Telegram.Bot.Helper.Sniffer;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.Payments;

namespace Telegram.Bot.Helper
{
    /// <summary>
    /// Telegram bot helper
    /// </summary>
    /// <typeparam name="TLocalizationModel">Class that contains localization fields</typeparam>
    public sealed class TelegramBotHelper<TLocalizationModel> where TLocalizationModel : class, new()
    {
        /// <summary>
        /// Callback data separator. Default is '~'. You can change it in constructor.
        /// </summary>
        public readonly char Separator;

        /// <summary>
        /// Original instance of telegram client
        /// </summary>
        private readonly ConcurrentDictionary<string, TelegramBotClient> _clients = new ConcurrentDictionary<string, TelegramBotClient>();

        private readonly List<CallbackQueryHandler<TLocalizationModel>> _callbackQueryHandlers = new List<CallbackQueryHandler<TLocalizationModel>>();
        private readonly List<MessageHandler<TLocalizationModel>> _messageHandlers = new List<MessageHandler<TLocalizationModel>>();

        private readonly ConcurrentDictionary<string, TLocalizationModel> _localizationModels = new ConcurrentDictionary<string, TLocalizationModel>();

        private readonly ConcurrentDictionary<int, ConcurrentQueue<ISniffer>> _sniffers = new ConcurrentDictionary<int, ConcurrentQueue<ISniffer>>();

        /// <summary>
        /// Verify user on every incoming message. If null, all verify statuses will be set to Unchecked.
        /// </summary>
        public Func<User, Task<Verify>> Verifying;

        /// <summary>
        /// Use this delegate to change user's IETF language code.
        /// </summary>
        public Func<User, Task<string>> SelectLanguage;

        /// <summary>
        /// Use this delegate to receive incoming InlineQuery updates
        /// </summary>
        public Func<InlineQuery, Verify, TLocalizationModel, Task> OnInlineQuery;

        /// <summary>
        /// Use this delegate to receive incoming ChosenInlineResult updates
        /// </summary>
        public Func<ChosenInlineResult, Verify, TLocalizationModel, Task> OnInlineResult;

        /// <summary>
        /// Use this delegate to receive incoming PreCheckoutQuery updates
        /// </summary>
        public Func<PreCheckoutQuery, Verify, TLocalizationModel, Task> OnPreCheckoutQuery;

        /// <summary>
        /// Use this delegate to receive incoming ShippingQuery updates
        /// </summary>
        public Func<ShippingQuery, Verify, TLocalizationModel, Task> OnShippingQuery;

        /// <summary>
        /// Settings for update handling
        /// </summary>
        public readonly TelegramBotHelperSettings Settings = new TelegramBotHelperSettings();

        /// <summary>
        /// Create new instance of TelegramBotHelper class.
        /// </summary>
        /// <param name="separator">Separator which will be used to split callback query data.</param>
        public TelegramBotHelper(char separator = '~')
        {
            Separator = separator;
        }

        /// <summary>
        /// Add localization models from directory where executing (.exe) file was started
        /// </summary>
        public void AddJsonLocalizationFromCurrentDirectory()
        {
            AddJsonLocalization(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        }

        /// <summary>
        /// Add localization models from custom directory path
        /// </summary>
        /// <param name="directoryPath">Path to directory where all json files are placed</param>
        public void AddJsonLocalization(string directoryPath)
        {
            var mapper = new LocalizationMapper<TLocalizationModel>(directoryPath);
            foreach (var (key, value) in mapper.GetLocalizationModels())
                _localizationModels.TryAdd(key, value);
        }

        /// <summary>
        /// Add localization model manually
        /// </summary>
        /// <param name="languageCode">IETF language code to add (ru, en, de, ...)</param>
        /// <param name="localizationModel">Model that contains localization for specified language</param>
        public void AddLocalizationModel(string languageCode, TLocalizationModel localizationModel)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
                throw new ArgumentException("Language code can't be null, empty or white-spaces", nameof(languageCode));
            if (_localizationModels.ContainsKey(languageCode))
                throw new ArgumentException("Localization for this language code already exists.", nameof(languageCode));

            _localizationModels.TryAdd(languageCode, localizationModel ?? throw new ArgumentNullException(nameof(localizationModel)));
        }

        /// <summary>
        /// Add sniffer for specified user
        /// </summary>
        public void AddSniffer(int userId, ISniffer sniffer)
        {
            if (_sniffers.TryGetValue(userId, out var q))
                q.Enqueue(sniffer);
            else
            {
                q = new ConcurrentQueue<ISniffer>();
                q.Enqueue(sniffer);
                _sniffers.TryAdd(userId, q);
            }
        }

        /// <summary>
        /// Process incoming update
        /// </summary>
        /// <param name="update">Incoming update</param>
        /// <param name="id">Identifier of TelegramBotClient</param>
        public async Task UpdateReceived(Update update, string id)
        {
            if (update == null || !_clients.TryGetValue(id, out var client))
                return;
            
            User from;
            switch (update.Type)
            {
                case UpdateType.CallbackQuery:
                    from = update.CallbackQuery.From;
                    break;
                
                case UpdateType.ChannelPost:
                    from = update.ChannelPost.From;
                    break;
                
                case UpdateType.ChosenInlineResult:
                    from = update.ChosenInlineResult.From;
                    break;
                
                case UpdateType.EditedChannelPost:
                    from = update.EditedChannelPost.From;
                    break;
                
                case UpdateType.EditedMessage:
                    from = update.EditedMessage.From;
                    break;
                
                case UpdateType.InlineQuery:
                    from = update.InlineQuery.From;
                    break;
                
                case UpdateType.Message:
                    from = update.Message.From;
                    if (!_sniffers.TryGetValue(from.Id, out var sniffers))
                        break;
                    if (sniffers.TryPeek(out var sniffer)
                        && await sniffer.RunSniffer(update.Message, client)
                        && sniffers.TryDequeue(out _)
                        && sniffers.Count == 0)
                        _sniffers.TryRemove(from.Id, out _);
                    break;
                
                case UpdateType.PreCheckoutQuery:
                    from = update.PreCheckoutQuery.From;
                    break;
                
                case UpdateType.ShippingQuery:
                    from = update.ShippingQuery.From;
                    break;
                
                default: return;
            }

            var lang = SelectLanguage == null ? from?.LanguageCode : await SelectLanguage(from);

            if (lang == null || !_localizationModels.ContainsKey(lang))
                lang = Settings.Localization.DefaultLocalizationKey ?? throw new NullReferenceException("Default localization key is null");

            if (!_localizationModels.TryGetValue(lang, out var localizationModel))
                throw new KeyNotFoundException($"Language code '{lang}' was not found");

            var v = Verifying != null ? await Verifying(from) : Verify.Unchecked;

            switch (update.Type)
            {
                case UpdateType.EditedMessage when !Settings.IgnoreEditedMessages:
                case UpdateType.Message when !Settings.IgnoreMessages:
                case UpdateType.ChannelPost when !Settings.IgnoreChannelPosts:
                case UpdateType.EditedChannelPost when !Settings.IgnoreEditedChannelPosts:
                    var message = update.Message
                                  ?? update.EditedMessage
                                  ?? update.ChannelPost
                                  ?? update.EditedChannelPost;
                    foreach (var messageHandler in _messageHandlers)
                    {
                        if (!messageHandler.Predicate(message) || !messageHandler.Verified.HasFlag(v))
                            continue;
                        await messageHandler.Callback(message, localizationModel);
                    }
                    break;
                
                case UpdateType.CallbackQuery:
                    var query = new CallbackQueryCommand(update.CallbackQuery.Data, Separator);
                    foreach (var callbackQueryHandler in _callbackQueryHandlers)
                    {
                        if (query.Equals(callbackQueryHandler.Command)
                            && callbackQueryHandler.Verified.HasFlag(v))
                            await callbackQueryHandler.Callback(new CallbackQueryInfo(update.CallbackQuery), query.Commands, localizationModel);
                    }
                    break;
                
                case UpdateType.InlineQuery when OnInlineQuery != null:
                    await OnInlineQuery(update.InlineQuery, v, localizationModel);
                    break;
                
                case UpdateType.ChosenInlineResult when OnInlineResult != null:
                    await OnInlineResult(update.ChosenInlineResult, v, localizationModel);
                    break;
                
                case UpdateType.PreCheckoutQuery when OnPreCheckoutQuery != null:
                    await OnPreCheckoutQuery(update.PreCheckoutQuery, v, localizationModel);
                    break;
                
                case UpdateType.ShippingQuery when OnShippingQuery != null:
                    await OnShippingQuery(update.ShippingQuery, v, localizationModel);
                    break;
            }
        }

        /// <summary>
        /// CallbackQuery handlers
        /// </summary>
        public void CallbackQueries(Action<CallbackQueryHandlerBuilder<TLocalizationModel>> builder)
        {
            builder(new CallbackQueryHandlerBuilder<TLocalizationModel>(_callbackQueryHandlers, Separator));
        }

        /// <summary>
        /// Text message handlers
        /// </summary>
        public void TextMessages(Action<TextMessageHandlerBuilder<TLocalizationModel>> builder)
        {
            builder(new TextMessageHandlerBuilder<TLocalizationModel>(_textMessageCallbacks));
        }

        /// <summary>
        /// Expression handlers for messages
        /// </summary>
        public void MessageExpressions(Action<MessageHandlerBuilder<TLocalizationModel>> builder)
        {
            builder(new MessageHandlerBuilder<TLocalizationModel>(_messageHandlers));
        }
    }
}
