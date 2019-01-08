using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Telegram.Bot.Helper.HandlerBuilders;
using Telegram.Bot.Helper.Handlers;
using Telegram.Bot.Helper.Languages;
using Telegram.Bot.Helper.Localization;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

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

        /// <summary>
        /// Verify user on every incoming message. If null, all verify statuses will be set to Unchecked.
        /// </summary>
        public Func<User, Task<Verify>> Verifying;

        /// <summary>
        /// Use this delegate to change user's IETF language code.
        /// </summary>
        public Func<User, Task<string>> SelectLanguage;

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
                throw new ArgumentException("Language code can't be null, empty or white-spaces", "languageCode");
            if (_localizationModels.ContainsKey(languageCode))
                throw new ArgumentException("Localization for this language code already exists.", "languageCode");

            _localizationModels.Add(languageCode, localizationModel ?? throw new ArgumentNullException("localizationModel"));
        }

        /// <summary>
        /// Process incoming update
        /// </summary>
        /// <param name="update">Incoming update</param>
        public async Task UpdateReceived(Update update)
        {
            if (update == null)
                return;

            TLocalizationModel localizationModel = null;

            switch (update.Type)
            {
                case UpdateType.Message:
                    {
                        var message = update.Message;

                        var lang = SelectLanguage == null
                            ? message.From.LanguageCode
                            : await SelectLanguage(message.From);
                        
                        if (lang == null || !_localizationModels.ContainsKey(lang))
                            lang = _localizationOptions.DefaultLocalizationKey;

                        if (!_localizationModels.TryGetValue(lang, out localizationModel))
                            throw new KeyNotFoundException($"Language code '{lang}' was not found");

                        var v = Verifying != null
                            ? await Verifying(message.From)
                            : Verify.Unchecked;

                        switch (message.Type)
                        {
                            case MessageType.Text:
                                {
                                    if (_textMessageCallbacks.Count == 0)
                                        goto default;

                                    var mCb = _textMessageCallbacks.Find(it =>
                                    {
                                        var value = it.Message.Compile();
                                        return value(localizationModel) == message.Text;
                                    });
                                    if (mCb != default)
                                    {
                                        if (mCb.Verified == Verify.Unchecked || mCb.Verified.HasFlag(v))
                                            await mCb.Callback(message, v, localizationModel);
                                        break;
                                    }

                                    goto default;
                                }
                            default:
                                {
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
                        }
                        break;
                    }
                case UpdateType.CallbackQuery:
                    {
                        var q = update.CallbackQuery;
                        
                        var message = q.Message;
                        var lang = SelectLanguage == null
                            ? message.From.LanguageCode
                            : await SelectLanguage(message.From);

                        if (lang == null || !_localizationModels.ContainsKey(lang))
                            lang = _localizationOptions.DefaultLocalizationKey;

                        if (!_localizationModels.TryGetValue(lang, out localizationModel))
                            throw new KeyNotFoundException($"Language code '{lang}' was not found");

                        var v = Verifying != null
                            ? await Verifying(q.From)
                            : Verify.Unchecked;

                        var c = new CallbackQueryCommand(q.Data, Separator);
                        foreach (var callbackQueryFunction in _callbackQueryFunctions)
                        {
                            if (!c.Equals(callbackQueryFunction.Command))
                                continue;

                            if (callbackQueryFunction.Verified == Verify.Unchecked || callbackQueryFunction.Verified.HasFlag(v))
                                await callbackQueryFunction.Callback(new CallbackQueryInfo(q), c.Commands, v, localizationModel);
                        }
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
