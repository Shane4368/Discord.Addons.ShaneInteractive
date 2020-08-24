using System;

namespace Discord.Addons.ShaneInteractive
{
    public class InteractiveServiceConfig
    {
        public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(15);
    }
}