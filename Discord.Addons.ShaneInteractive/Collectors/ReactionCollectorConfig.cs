using Discord.WebSocket;
using System;

namespace Discord.Addons.ShaneInteractive.Collectors
{
    public sealed class ReactionCollectorConfig : CollectorConfig
    {
        public ReactionCollectorConfig(BaseSocketClient client) : base(client) { }

        public int? MaxUsers { get; set; }
        public int? MaxEmotes { get; set; }

        /// <summary>
        /// The message to collect reactions from.
        /// </summary>
        public IUserMessage Message { get; set; }

        internal override void Validate()
        {
            if (MaxUsers <= 0)
                throw new ArgumentException("Value must be greater than 0", nameof(MaxUsers));

            if (MaxEmotes <= 0)
                throw new ArgumentException("Value must be greater than 0", nameof(MaxEmotes));

            if (Message == null)
                throw new ArgumentNullException(nameof(Message));

            base.Validate();
        }
    }
}