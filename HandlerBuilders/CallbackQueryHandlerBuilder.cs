using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Helper.Handlers;

namespace Telegram.Bot.Helper.HandlerBuilders
{
    /// <summary>
    /// Callback query builder
    /// </summary>
    public class CallbackQueryHandlerBuilder<TLocalizationModel> where TLocalizationModel : class, new()
    {
        private readonly List<CallbackQueryHandler<TLocalizationModel>> _callbacks;
        private readonly char _separator;

        internal CallbackQueryHandlerBuilder(List<CallbackQueryHandler<TLocalizationModel>> callbacks, char separator)
        {
            _callbacks = callbacks;
            _separator = separator;
        }

        public Func<CallbackQueryInfo, string[], Verify, TLocalizationModel, Task> this[string data, Verify verified = Verify.Unchecked]
        {
            set => _callbacks.Add(new CallbackQueryHandler<TLocalizationModel>(value, data, _separator, verified));
        }

        public Func<CallbackQueryInfo, string[], Verify, TLocalizationModel, Task> this[string[] dataItems, Verify verified = Verify.Unchecked]
        {
            set
            {
                foreach (var data in dataItems)
                    _callbacks.Add(new CallbackQueryHandler<TLocalizationModel>(value, data, _separator, verified));
            }
        }
    }
}
