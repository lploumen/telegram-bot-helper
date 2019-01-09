using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Telegram.Bot.Helper.Sniffer
{
    /// <summary>
    /// Allows you to temporary intercept specified types of update and run your own logic
    /// </summary>
    public interface ISniffer
    {
        /// <summary>
        /// Filter for sniffer so only specified update types will be intercepted.
        /// </summary>
        IEnumerable<UpdateType> FilterTypes();

        /// <summary>
        /// If returns true, sniffer will be removed.
        /// </summary>
        /// <param name="update">Incoming update that was intercepted</param>
        Task<bool> ValidateAsync(Update update);

        /// <summary>
        /// This method will be called when <see cref="ValidateAsync"/> returns true.
        /// </summary>
        /// <param name="update">Incoming update that was intercepted</param>
        Task OnSuccessAsync(Update update);

        /// <summary>
        /// This method will be called when <see cref="ValidateAsync"/> returns false.
        /// </summary>
        /// <param name="update">Incoming update that was intercepted</param>
        Task OnFailureAsync(Update update);
    }
}
