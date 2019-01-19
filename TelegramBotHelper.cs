using System;
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
        public readonly TelegramBotClient Client;

        private readonly List<CallbackQueryHandler<TLocalizationModel>> _callbackQueryFunctions = new List<CallbackQueryHandler<TLocalizationModel>>();
        private readonly List<TextMessageHandler<TLocalizationModel>> _textMessageCallbacks = new List<TextMessageHandler<TLocalizationModel>>();
        private readonly List<MessageExpressionHandler<TLocalizationModel>> _messageExpressionCallbacks = new List<MessageExpressionHandler<TLocalizationModel>>();

        private readonly Dictionary<string, TLocalizationModel> _localizationModels = new Dictionary<string, TLocalizationModel>();

        private readonly Dictionary<int, Queue<ISniffer>> _sniffers = new Dictionary<int, Queue<ISniffer>>();

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
        public Func<InlineQuery, Verify, TLocalizationModel, Task> ReceivedInlineQuery;

        /// <summary>
        /// Use this delegate to receive incoming ChosenInlineResult updates
        /// </summary>
        public Func<ChosenInlineResult, Verify, TLocalizationModel, Task> ReceivedInlineResult;

        /// <summary>
        /// Use this delegate to receive incoming PreCheckoutQuery updates
        /// </summary>
        public Func<PreCheckoutQuery, Verify, TLocalizationModel, Task> ReceivedPreCheckoutQuery;

        /// <summary>
        /// Use this delegate to receive incoming ShippingQuery updates
        /// </summary>
        public Func<ShippingQuery, Verify, TLocalizationModel, Task> ReceivedShippingQuery;

        /// <summary>
        /// Settings for update handling
        /// </summary>
        public readonly TelegramBotHelperSettings Settings = new TelegramBotHelperSettings();

        /// <summary>
        /// Create new instance of TelegramBotHelper class.
        /// </summary>
        /// <param name="client">Initialize TelegramBotClient to use by helper</param>
        /// <param name="separator">Separator which will be used to split callback query data.</param>
        public TelegramBotHelper(TelegramBotClient client, in char separator = '~')
        {
            Separator = separator;
            Client = client ?? throw new ArgumentNullException(nameof(client));
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
                _localizationModels.Add(key, value);
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

            _localizationModels.Add(languageCode, localizationModel ?? throw new ArgumentNullException(nameof(localizationModel)));
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
                q = new Queue<ISniffer>(1);
                q.Enqueue(sniffer);
                _sniffers.Add(userId, q);
            }
        }

        /// <summary>
        /// Process incoming update
        /// </summary>
        /// <param name="update">Incoming update</param>
        public async Task UpdateReceived(Update update)
        {
            if (update == null)
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

                    var sniffer = sniffers.Peek();
                    if (await sniffer.RunSniffer(update.Message, Client))
                    {
                        sniffers.Dequeue();
                        if (sniffers.Count == 0)
                            _sniffers.Remove(from.Id);
                    }
                    return;
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
                case UpdateType.EditedMessage when !Settings.IgnoreEditedPrivateMessages:
                case UpdateType.Message when !Settings.IgnorePrivateMessages:
                case UpdateType.ChannelPost when !Settings.IgnoreChannelPosts:
                case UpdateType.EditedChannelPost when !Settings.IgnoreEditedChannelPosts:
                    {
                        Message message = update.Type == UpdateType.Message ? update.Message
                            : update.Type == UpdateType.EditedMessage ? update.EditedMessage
                            : update.Type == UpdateType.ChannelPost ? update.ChannelPost
                            : update.EditedChannelPost;
                        
                        if (message.Type == MessageType.Text && _textMessageCallbacks.Count > 0)
                        {
                            var mCb = _textMessageCallbacks.Find(it => it.Message(localizationModel) == message.Text);

                            if (mCb != default)
                            {
                                if (mCb.Verified == Verify.Unchecked || mCb.Verified.HasFlag(v))
                                    await mCb.Callback(message, localizationModel);
                                return;
                            }
                        }

                        foreach (var messageExpressionCallback in _messageExpressionCallbacks)
                        {
                            if (!messageExpressionCallback.Expression(message))
                                continue;

                            if (messageExpressionCallback.Verified != Verify.Unchecked
                                && !messageExpressionCallback.Verified.HasFlag(v))
                                continue;

                            await messageExpressionCallback.Callback(message, localizationModel);
                        }

                        break;
                    }
                case UpdateType.CallbackQuery:
                    {
                        var c = new CallbackQueryCommand(update.CallbackQuery.Data, Separator);
                        foreach (var callbackQueryFunction in _callbackQueryFunctions)
                        {
                            if (!c.Equals(callbackQueryFunction.Command))
                                continue;

                            if (callbackQueryFunction.Verified == Verify.Unchecked || callbackQueryFunction.Verified.HasFlag(v))
                                await callbackQueryFunction.Callback(new CallbackQueryInfo(update.CallbackQuery), c.Commands, localizationModel);
                        }
                        break;
                    }
                case UpdateType.InlineQuery:
                    {
                        if (ReceivedInlineQuery != null)
                            await ReceivedInlineQuery(update.InlineQuery, v, localizationModel);
                        break;
                    }
                case UpdateType.ChosenInlineResult:
                    {
                        if (ReceivedInlineResult != null)
                            await ReceivedInlineResult(update.ChosenInlineResult, v, localizationModel);
                        break;
                    }
                case UpdateType.PreCheckoutQuery:
                    {
                        if (ReceivedPreCheckoutQuery != null)
                            await ReceivedPreCheckoutQuery(update.PreCheckoutQuery, v, localizationModel);
                        break;
                    }
                case UpdateType.ShippingQuery:
                    {
                        if (ReceivedShippingQuery != null)
                            await ReceivedShippingQuery(update.ShippingQuery, v, localizationModel);
                        break;
                    }
            }
        }

        /// <summary>
        /// CallbackQuery handlers
        /// </summary>
        public void CallbackQueries(Action<CallbackQueryHandlerBuilder<TLocalizationModel>> builder)
        {
            builder(new CallbackQueryHandlerBuilder<TLocalizationModel>(_callbackQueryFunctions, Separator));
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
        public void MessageExpressions(Action<MessageExpressionHandlerBuilder<TLocalizationModel>> builder)
        {
            builder(new MessageExpressionHandlerBuilder<TLocalizationModel>(_messageExpressionCallbacks));
        }
    }
}
