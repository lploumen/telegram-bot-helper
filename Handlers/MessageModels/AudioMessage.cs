using Telegram.Bot.Types;

namespace Telegram.Bot.Helper.Handlers.MessageModels
{
    public class AudioMessage : MediaMessageBase
    {
        internal AudioMessage(Message message) : base(message) { }

        /// <summary>
        /// Optional. Description is an audio file, information about the file
        /// </summary>
        public Audio Audio => _message.Audio;
    }
}