using System;
using System.Collections.Generic;
using Telegram.Bot.Types;

namespace Telegram.Bot.Helper.Handlers.MessageModels
{
    public abstract class MediaMessageBase
    {
        private protected Message _message;

        private protected MediaMessageBase(Message message) => _message = message;

        /// <summary>Unique message identifier</summary>
        public int MessageId => _message.MessageId;

        /// <summary>Sender</summary>
        public User From => _message.From;

        /// <summary>Date the message was sent</summary>
        public DateTime Date => _message.Date;

        /// <summary>Conversation the message belongs to</summary>
        public Chat Chat => _message.Chat;

        /// <summary>
        /// Optional. For forwarded messages, sender of the original message
        /// </summary>
        public User ForwardFrom => _message.ForwardFrom;

        /// <summary>
        /// Optional. For messages forwarded from a channel, information about the original channel
        /// </summary>
        public Chat ForwardFromChat => _message.ForwardFromChat;

        /// <summary>
        /// Optional. For forwarded channel posts, identifier of the original message in the channel
        /// </summary>
        public int ForwardFromMessageId => _message.ForwardFromMessageId;

        /// <summary>
        /// Optional. For messages forwarded from channels, signature of the post author if present
        /// </summary>
        public string ForwardSignature => _message.ForwardSignature;

        /// <summary>
        /// Optional. For forwarded messages, date the original message was sent in Unix time
        /// </summary>
        public DateTime? ForwardDate => _message.ForwardDate;

        /// <summary>
        /// Optional. For replies, the original message. Note that the Description object in this field will not contain further reply_to_message fields even if it itself is a reply.
        /// </summary>
        public Message ReplyToMessage => _message.ReplyToMessage;

        /// <summary>
        /// Optional. Date the message was last edited in Unix time
        /// </summary>
        public DateTime? EditDate => _message.EditDate;

        /// <summary>
        /// Optional. Signature of the post author for messages in channels
        /// </summary>
        public string AuthorSignature => _message.AuthorSignature;

        /// <summary>
        /// Optional. For messages with a caption, special entities like usernames, URLs, bot commands, etc. that appear in the caption
        /// </summary>
        public MessageEntity[] CaptionEntities => _message.CaptionEntities;

        /// <summary>Gets the caption entity values.</summary>
        public IEnumerable<string> CaptionEntityValues => _message.CaptionEntityValues;

        /// <summary>Caption for the photo or video</summary>
        public string Caption => _message.Caption;
    }
}