using Discord.WebSocket;
using System;

namespace Discord.Addons.ShaneInteractive.Collectors
{
    public sealed class MessageCollectorConfig : CollectorConfig
    {
        public MessageCollectorConfig(BaseSocketClient client) : base(client) { }

        /// <summary>
        /// The channel to collect messages from.
        /// </summary>
        public IMessageChannel Channel { get; set; }
        public int? MaxMessages { get; set; }

        internal override void Validate()
        {
            if (Channel == null) throw new ArgumentNullException(nameof(Channel));

            if (MaxMessages <= 0)
                throw new ArgumentException("Value must be greater than 0", nameof(MaxMessages));

            base.Validate();
        }
    }
}