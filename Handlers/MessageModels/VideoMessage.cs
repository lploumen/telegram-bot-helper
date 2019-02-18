using Telegram.Bot.Types;

namespace Telegram.Bot.Helper.Handlers.MessageModels
{
    public sealed class VideoMessage : MediaMessageBase
    {
        internal VideoMessage(Message message) : base(message) { }

        /// <summary>
        /// Description is a voice message, information about the file
        /// </summary>
        public Video Video => _message.Video;
    }
}