using Telegram.Bot.Types;

namespace Telegram.Bot.Helper.Handlers.MessageModels
{
    public sealed class AnimationMessage : MediaMessageBase
    {
        internal AnimationMessage(Message message) : base(message) { }

        /// <summary>
        /// Optional. Message is an animation, information about the animation. For backward compatibility, when this
        /// field is set, the document field will also be set
        /// </summary>
        public Animation Animation => _message.Animation;

        /// <summary>
        /// Optional. Description is a general file, information about the file
        /// </summary>
        public Document Document => _message.Document;
    }
}