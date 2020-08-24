using Discord.WebSocket;
using System;

namespace Discord.Addons.ShaneInteractive.Collectors
{
    public abstract class CollectorConfig
    {
        internal CollectorConfig(BaseSocketClient client)
           => Client = client ?? throw new ArgumentNullException(nameof(client));

        internal BaseSocketClient Client { get; }

        public int? Max { get; set; }

        /// <summary>
        /// Whether or not to dispose data when it's deleted.
        /// </summary>
        public bool Dispense { get; set; }

        /// <summary>
        /// If true, resets <see cref="Timeout"/> if active.
        /// </summary>
        public bool ResetTimeout { get; set; }
        public TimeSpan? Timeout { get; set; }

        internal virtual void Validate()
        {
            if (Max <= 0)
                throw new ArgumentException("Value must be greater than 0", nameof(Max));

            if (Timeout < TimeSpan.Zero)
                throw new ArgumentException("Value must be positive", nameof(Timeout));

            if (ResetTimeout && !Timeout.HasValue)
                throw new Exception($"Timeout cannot be null if {nameof(ResetTimeout)}=true");
        }
    }
}