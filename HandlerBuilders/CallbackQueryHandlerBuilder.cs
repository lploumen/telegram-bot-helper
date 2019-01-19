using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Helper.Handlers;

namespace Telegram.Bot.Helper.HandlerBuilders
{
    /// <summary>
    /// Builder for callback query handlers
    /// </summary>
    /// <typeparam name="TLocalizationModel">Localization model</typeparam>
    public sealed class CallbackQueryHandlerBuilder<TLocalizationModel> where TLocalizationModel : class, new()
    {
        private readonly List<CallbackQueryHandler<TLocalizationModel>> _callbacks;
        private readonly char _separator;

        internal CallbackQueryHandlerBuilder(in List<CallbackQueryHandler<TLocalizationModel>> callbacks, in char separator)
        {
            _callbacks = callbacks;
            _separator = separator;
        }

        public Func<CallbackQueryInfo, string[], TLocalizationModel, Task> this[string data, Verify verified = Verify.Unchecked]
        {
            set
            {
                if (data == null)
                    throw new ArgumentNullException(nameof(data));
                if (value == null)
                    throw new ArgumentNullException("value");

                _callbacks.Add(new CallbackQueryHandler<TLocalizationModel>(value, data, _separator, verified));
            }
        }

        public Func<CallbackQueryInfo, string[], TLocalizationModel, Task> this[string[] dataItems, Verify verified = Verify.Unchecked]
        {
            set
            {
                if (dataItems == null)
                    throw new ArgumentNullException(nameof(dataItems));
                if (value == null)
                    throw new ArgumentNullException("value");

                int index = 0;
                foreach (var dataItem in dataItems)
                {
                    if (dataItem == null)
                        throw new ArgumentNullException($"{nameof(dataItems)}[{index}]");

                    _callbacks.Add(new CallbackQueryHandler<TLocalizationModel>(value, dataItem, _separator, verified));
                    ++index;
                }
            }
        }
    }
}
