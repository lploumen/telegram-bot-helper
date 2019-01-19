using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Telegram.Bot.Helper.Sniffer
{
    /// <summary>
    /// Allows you to temporary intercept incoming messages and run your own logic for them.
    /// </summary>
    public interface ISniffer
    {
        /// <summary>
        /// If returns true, sniffer will be removed.
        /// </summary>
        /// <param name="message">Incoming message that was intercepted</param>
        bool Validate(Message message);

        /// <summary>
        /// This method will be called when <see cref="Validate"/> returns true.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message">Incoming message that was intercepted</param>
        Task OnSuccessAsync(TelegramBotClient client, Message message);

        /// <summary>
        /// This method will be called when <see cref="Validate"/> returns false.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message">Incoming message that was intercepted</param>
        Task OnFailureAsync(TelegramBotClient client, Message message);
    }
}
