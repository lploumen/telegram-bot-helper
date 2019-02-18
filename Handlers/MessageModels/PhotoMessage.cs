using Telegram.Bot.Types;

namespace Telegram.Bot.Helper.Handlers.MessageModels
{
    public sealed class PhotoMessage : MediaMessageBase
    {
        internal PhotoMessage(Message originalMessage) : base(originalMessage) { }

        /// <summary>
        /// Optional. Description is a photo, available sizes of the photo
        /// </summary>
        public PhotoSize[] Photo => _message.Photo;
    }
}