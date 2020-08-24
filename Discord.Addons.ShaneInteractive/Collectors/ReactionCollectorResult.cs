using System.Collections.Generic;

namespace Discord.Addons.ShaneInteractive.Collectors
{
    public sealed class ReactionCollectorResult
    {
        internal ReactionCollectorResult(IEmote emote) => Emote = emote;

        public IEmote Emote { get; }
        public IReadOnlyCollection<ulong> UserIds { get; internal set; }

        public override bool Equals(object obj) => Emote.Equals(obj);
        public override int GetHashCode() => Emote.GetHashCode();
    }
}