using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Helper.Actions;
using Telegram.Bot.Helper.Languages;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.Helper
{
    public class TelegramBotHelper
    {
        /// <summary>
        /// Callback data separator. Default is "~". You can change it in constructor.
        /// </summary>
        public readonly string Separator;

        /// <summary>
        /// Telegram client
        /// </summary>
        public readonly TelegramBotClient Client;

        private List<BotInline> _inlineCallbacks = new List<BotInline>();
        private List<BotTextMessage> _messageCallbacks = new List<BotTextMessage>();
        private List<BotExpression> _expressionCallbacks = new List<BotExpression>();

        private Dictionary<string, Dictionary<string, string>> _localizationManagers = new Dictionary<string, Dictionary<string, string>>();
        private readonly LocalizationOptions _localizationOptions;

        /// <summary>
        /// You can create your own update handler using this delegate.
        /// </summary>
        public Func<Update, List<BotInline>, List<BotTextMessage>, List<BotExpression>,
            Dictionary<string, Dictionary<string, string>>, Task> CustomUpdateReceived;

        /// <summary>
        /// Verify user on every incoming message. If null, all verify statuses will be set to Unchecked.
        /// </summary>
        public Func<User, Task<Verify>> Verifying;

        /// <summary>
        /// Use this delegate to change user's current language.
        /// </summary>
        public Func<User, Task<string>> LanguageSelection;

        /// <summary>
        /// Create new instance of TelegramBotHelper class.
        /// </summary>
        /// <param name="initializer">Initialize TelegramBotClient to use by helper</param>
        /// <param name="localizationOptions">Localization options. Use null to disable localization.</param>
        /// <param name="separator">Separator which will be used to split callback query data. Default is "~".</param>
        public TelegramBotHelper(Func<TelegramBotClient> initializer, LocalizationOptions localizationOptions = null, string separator = "~")
        {
            if (initializer == null)
                throw new ArgumentNullException("initializer");
            if (string.IsNullOrWhiteSpace(separator))
                throw new ArgumentException("Separator cannot be null, empty string or white-spaces", "separator");

            Client = initializer();
            if (Client == null)
                throw new ArgumentException("Initializer must not return null", "initializer");

            Separator = separator;
            _localizationOptions = localizationOptions;
        }

        /// <summary>
        /// Adding localization data from json files using specified language codes
        /// </summary>
        /// <param name="basePath">Base path of directory where all files are placed</param>
        /// <param name="languageCodes">All language codes to add. If language code is en-US, it will search for en-US.json file in basePath directory.</param>
        public void AddLocalizationFromJsonFiles(string basePath, params string[] languageCodes)
        {
            if (_localizationOptions == null)
                throw new NotSupportedException("Localization is disabled. You can enable it using localizationOptions parameter in constructor.");

            foreach (var languageCode in languageCodes)
            {
                if (_localizationManagers.ContainsKey(languageCode))
                    throw new ArgumentException($"Localization for {languageCode} already exists.", "languageCode");
                
                _localizationManagers[languageCode] = BotLocalizationManagerExtensions.ReadLocalizationDataFromFile(basePath, languageCode, null);
            }
        }

        /// <summary>
        /// Adding localization data from json files using specified language codes and file encoding
        /// </summary>
        /// <param name="basePath">Base path of directory where all files are placed</param>
        /// <param name="enc">File encoding</param>
        /// <param name="languageCodes">All language codes to add. If language code is en-US, it will search for en-US.json file in basePath directory.</param>
        public void AddLocalizationFromJsonFiles(string basePath, Encoding enc, params string[] languageCodes)
        {
            if (_localizationOptions == null)
                throw new NotSupportedException("Localization is disabled. You can enable it using localizationOptions parameter in constructor.");

            foreach (var languageCode in languageCodes)
            {
                if (_localizationManagers.ContainsKey(languageCode))
                    throw new ArgumentException($"Localization for {languageCode} already exists.", "languageCode");

                _localizationManagers[languageCode] = BotLocalizationManagerExtensions.ReadLocalizationDataFromFile(basePath, languageCode, enc);
            }
        }

        /// <summary>
        /// Adding localization data as dictionary using specified language code
        /// </summary>
        /// <param name="languageCode">Language code to add.</param>
        public void AddLocalization(string languageCode, Dictionary<string, string> data)
        {
            if (_localizationOptions == null)
                throw new NotSupportedException("Localization is disabled. You can enable it using localizationOptions parameter in constructor.");
            if (string.IsNullOrWhiteSpace(languageCode))
                throw new ArgumentException("Language code can't be null, empty or white-spaces", "languageCode");
            if (_localizationManagers.ContainsKey(languageCode))
                throw new ArgumentException("Localization for this language code already exists.", "languageCode");

            _localizationManagers[languageCode] = data ?? throw new ArgumentNullException("data");
        }

        /// <summary>
        /// Process new telegram update.
        /// </summary>
        /// <param name="update">Incoming update</param>
        public async Task UpdateReceived(Update update)
        {
            if (update == null)
                return;

            if (CustomUpdateReceived != null)
            {
                await CustomUpdateReceived(update,
                    _inlineCallbacks,
                    _messageCallbacks,
                    _expressionCallbacks,
                    _localizationOptions != null ? _localizationManagers : null);
                return;
            }

            Dictionary<string, string> languageDictionary = null;

            switch (update.Type)
            {
                case UpdateType.Message:
                    {
                        var message = update.Message;
                        if (message == null)
                            return;

                        if (_localizationOptions != null)
                        {
                            var lang = LanguageSelection == null
                                ? message.From.LanguageCode
                                : await LanguageSelection(message.From);

                            if (string.IsNullOrWhiteSpace(lang) || !_localizationManagers.ContainsKey(lang))
                            {
                                if (!_localizationManagers.ContainsKey(_localizationOptions.DefaultLocalizationKey))
                                    throw new KeyNotFoundException("Default language key was not found in dictionary");

                                lang = _localizationOptions.DefaultLocalizationKey;
                            }
                            languageDictionary = _localizationManagers[lang];
                        }

                        var v = Verifying != null
                            ? await Verifying(message.From)
                            : Verify.Unchecked;

                        switch (message.Type)
                        {
                            case MessageType.Text:
                                {
                                    if (_messageCallbacks.Count == 0)
                                        goto default;

                                    BotTextMessage mCb = mCb = _messageCallbacks.Find(it => it.Message == message.Text);
                                    if (mCb == null && languageDictionary != null)
                                        mCb = _messageCallbacks.Find(it => languageDictionary.TryGetValue(it.Message, out var mesVal)
                                            && message.Text == mesVal);
                                    if (mCb != default)
                                    {
                                        if (mCb.Verified == Verify.Unchecked || mCb.Verified.HasFlag(v))
                                            await mCb.Callback(message, v, languageDictionary);
                                        break;
                                    }

                                    goto default;
                                }
                            default:
                                {
                                    foreach (var expressionCallback in _expressionCallbacks)
                                    {
                                        if (!expressionCallback.Expression(message))
                                            continue;

                                        if (expressionCallback.Verified != Verify.Unchecked
                                            && !expressionCallback.Verified.HasFlag(v))
                                            continue;

                                        await expressionCallback.Callback(message, v, languageDictionary);
                                    }
                                    break;
                                }
                        }
                        break;
                    }
                case UpdateType.CallbackQuery:
                    {
                        var q = update.CallbackQuery;
                        var v = Verifying != null
                            ? await Verifying(q.From)
                            : Verify.Unchecked;

                        if (_localizationOptions != null)
                        {
                            var message = q.Message;
                            var lang = LanguageSelection == null
                                ? message.From.LanguageCode
                                : await LanguageSelection(message.From);

                            if (string.IsNullOrWhiteSpace(lang) || !_localizationManagers.ContainsKey(lang))
                            {
                                if (!_localizationManagers.ContainsKey(_localizationOptions.DefaultLocalizationKey))
                                    throw new KeyNotFoundException("Default language key was not found in dictionary");

                                lang = _localizationOptions.DefaultLocalizationKey;
                            }
                            languageDictionary = _localizationManagers[lang];
                        }

                        var c = new InlineCommand(q.Data, Separator);
                        foreach (var inlineCallback in _inlineCallbacks)
                        {
                            if (!c.Equals(inlineCallback.Command))
                                continue;

                            if (inlineCallback.Verified == Verify.Unchecked || inlineCallback.Verified.HasFlag(v))
                            {
                                await inlineCallback.Callback(new CallbackQueryInfo(q), c.Commands, v, languageDictionary);
                                break;
                            }
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// Inline handlers
        /// </summary>
        public void Inlines(Action<BotInline.Builder> builder)
        {
            builder(new BotInline.Builder(ref _inlineCallbacks, Separator));
        }

        /// <summary>
        /// Message handlers
        /// </summary>
        public void Messages(Action<BotTextMessage.Builder> builder)
        {
            builder(new BotTextMessage.Builder(ref _messageCallbacks));
        }

        /// <summary>
        /// Expression handlers
        /// </summary>
        public void Expressions(Action<BotExpression.Builder> builder)
        {
            builder(new BotExpression.Builder(ref _expressionCallbacks));
        }

        /// <summary>
        /// All handlers together (messages, inlines, expressions)
        /// </summary>
        public void Handlers(Action<BotTextMessage.Builder, BotInline.Builder, BotExpression.Builder> builder)
        {
            builder(new BotTextMessage.Builder(ref _messageCallbacks),
                new BotInline.Builder(ref _inlineCallbacks, Separator),
                new BotExpression.Builder(ref _expressionCallbacks));
        }
    }
}
