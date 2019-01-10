using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Helper.HandlerBuilders;
using Telegram.Bot.Helper.Handlers;
using Telegram.Bot.Helper.Languages;
using Telegram.Bot.Helper.Localization;
using Telegram.Bot.Helper.Sniffer;
using Telegram.Bot.Helper.Widgets;
using Telegram.Bot.Helper.Widgets.Data;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.Payments;

namespace Telegram.Bot.Helper
{
    /// <summary>
    /// Telegram bot helper
    /// </summary>
    /// <typeparam name="TLocalizationModel">Class that contains localization fields</typeparam>
    public class TelegramBotHelper<TLocalizationModel> where TLocalizationModel : class, new()
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
        private readonly LocalizationOptions _localizationOptions;

        private readonly Dictionary<int, List<ISniffer>> _sniffers = new Dictionary<int, List<ISniffer>>();

        public WidgetSettings<TLocalizationModel> WidgetSettings = new WidgetSettings<TLocalizationModel>();

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
        /// Create new instance of TelegramBotHelper class.
        /// </summary>
        /// <param name="initializer">Initialize TelegramBotClient to use by helper</param>
        /// <param name="localizationOptions">Localization options. If null, will be used 'en' as default localization key.</param>
        /// <param name="separator">Separator which will be used to split callback query data.</param>
        public TelegramBotHelper(Func<TelegramBotClient> initializer, LocalizationOptions localizationOptions, char separator = '~')
        {
            if (initializer == null)
                throw new ArgumentNullException(nameof(initializer));

            Separator = separator;
            _localizationOptions = localizationOptions ?? throw new ArgumentNullException(nameof(localizationOptions));
            if (_localizationOptions.DefaultLocalizationKey == null)
                throw new ArgumentNullException(nameof(_localizationOptions.DefaultLocalizationKey));

            Client = initializer();
            if (Client == null)
                throw new ArgumentException("Initializer must not return null", nameof(initializer));

            InitializeRegisteredCallbackQueryHandlers();

            Client.OnUpdate += async (sender, e) =>
                await UpdateReceived(e.Update);
        }

        private void InitializeRegisteredCallbackQueryHandlers()
        {
            var separatorStr = Separator.ToString();
            CallbackQueries(_q =>
            {
                _q["ignore"] = (q, c, v, l) =>
                {
                    return Client.AnswerCallbackQueryAsync(q.Query.Id, cacheTime: 86400);
                };
                _q[string.Join(separatorStr, "widget", "calendar", " ", " ")] = async (q, c, v, l) =>
                {
                    if (WidgetSettings.Calendar == null)
                        throw new NullReferenceException($"{nameof(WidgetSettings)}.{nameof(WidgetSettings.Calendar)} is null");

                    WidgetSettings.Calendar.EnsureSettingsAreCorrect();

                    if (!int.TryParse(c[2], out var year) || !int.TryParse(c[3], out var month))
                        return;

                    var newData = new CalendarData
                    {
                        CurrentPosition = (year, month),
                        MaxAllowedDate = await WidgetSettings.Calendar.MaxAllowedDate(q.Query.From),
                        MinAllowedDate = await WidgetSettings.Calendar.MinAllowedDate(q.Query.From)
                    };

                    await Client.EditMessageReplyMarkupAsync(q.ChatId, q.MessageId,
                        new CalendarWidget<TLocalizationModel>(WidgetSettings.Calendar, newData, Separator, l));
                };
                _q[string.Join(separatorStr, "widget", "calendar", "", "", "")] = async (q, c, v, l) =>
                {
                    if (!int.TryParse(c[2], out var year) || !int.TryParse(c[3], out var month) || !int.TryParse(c[4], out var day))
                        return;

                    await Client.DeleteMessageAsync(q.ChatId, q.MessageId);
                    await Client.SendTextMessageAsync(q.ChatId, $"Selected date: {year}-{month}-{day}");
                    await WidgetSettings.Calendar.DateSelected((year, month, day), q.Query.From, v, l);
                };
            });
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
        /// Add sniffer for specified chat id
        /// </summary>
        public void AddSniffer(int userId, ISniffer sniffer)
        {
            if (_sniffers.ContainsKey(userId))
                _sniffers[userId].Add(sniffer);
            else _sniffers.Add(userId, new List<ISniffer> { sniffer });
        }

        /// <summary>
        /// Send calendar widget to specified user
        /// </summary>
        public async Task SendCalendarAsync(ChatId chatId, string text, TLocalizationModel localizationModel, CalendarData calendarData, User user,
            ParseMode parseMode = ParseMode.Default, bool disableWebPagePreview = false, bool disableNotification = false, int replyToMessageId = 0,
            CancellationToken cancellationToken = default)
        {
            if (WidgetSettings.Calendar == null)
                throw new NullReferenceException($"{nameof(WidgetSettings)}.{nameof(WidgetSettings.Calendar)} is null");
            if (localizationModel == null)
                throw new ArgumentNullException(nameof(localizationModel));

            WidgetSettings.Calendar.EnsureSettingsAreCorrect();

            calendarData.MaxAllowedDate = await WidgetSettings.Calendar.MaxAllowedDate(user);
            calendarData.MinAllowedDate = await WidgetSettings.Calendar.MinAllowedDate(user);

            await Client.SendTextMessageAsync(chatId, text, parseMode, disableWebPagePreview, disableNotification, replyToMessageId,
                new CalendarWidget<TLocalizationModel>(WidgetSettings.Calendar, calendarData, Separator, localizationModel), cancellationToken);
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
                    break;
                case UpdateType.PreCheckoutQuery:
                    from = update.PreCheckoutQuery.From;
                    break;
                case UpdateType.ShippingQuery:
                    from = update.ShippingQuery.From;
                    break;
                default: return;
            }

            if (await _sniffers.RunSniffers(from.Id, update))
                return;

            var lang = SelectLanguage == null ? from?.LanguageCode : await SelectLanguage(from);

            if (lang == null || !_localizationModels.ContainsKey(lang))
                lang = _localizationOptions.DefaultLocalizationKey;

            if (!_localizationModels.TryGetValue(lang, out var localizationModel))
                throw new KeyNotFoundException($"Language code '{lang}' was not found");

            var v = Verifying != null ? await Verifying(from) : Verify.Unchecked;

            switch (update.Type)
            {
                case UpdateType.EditedMessage:
                case UpdateType.Message:
                case UpdateType.ChannelPost:
                case UpdateType.EditedChannelPost:
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
                                    await mCb.Callback(message, v, localizationModel);
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

                            await messageExpressionCallback.Callback(message, v, localizationModel);
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
                                await callbackQueryFunction.Callback(new CallbackQueryInfo(update.CallbackQuery), c.Commands, v, localizationModel);
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
