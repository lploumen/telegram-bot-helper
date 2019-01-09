using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Telegram.Bot.Helper.Sniffer
{
    internal static class SnifferExtensions
    {
        internal static async Task<bool> RunSniffers(this Dictionary<int, List<ISniffer>> sniffersDictionary, int userId, Update update)
        {
            if (!sniffersDictionary.TryGetValue(userId, out var sniffers))
                return false;

            foreach (var sniffer in new List<ISniffer>(sniffers))
            {
                var filters = sniffer.FilterTypes();
                bool ok = false;
                foreach (var filter in filters)
                {
                    if (filter == update.Type)
                    {
                        ok = true;
                        break;
                    }
                }

                if (!ok) continue;

                var validate = await sniffer.ValidateAsync(update);
                if (validate)
                {
                    if (sniffers.Count > 1)
                        sniffers.Remove(sniffer);
                    else sniffersDictionary.Remove(userId);

                    await sniffer.OnSuccessAsync(update);
                }
                else await sniffer.OnFailureAsync(update);

                return true;
            }

            return false;
        }
    }
}
