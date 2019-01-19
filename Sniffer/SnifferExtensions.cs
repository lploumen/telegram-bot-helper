using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Telegram.Bot.Helper.Sniffer
{
    internal static class SnifferExtensions
    {
        internal static Task<bool> RunSniffer(this ISniffer snifferAsync, Message message, TelegramBotClient client)
        {
            var valid = snifferAsync.Validate(message);

            var t = valid ? snifferAsync.OnSuccessAsync(client, message) : snifferAsync.OnFailureAsync(client, message);

            return t.ContinueWith(_ => valid);
        }
    }
}
