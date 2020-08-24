namespace Discord.Addons.ShaneInteractive.Pagination
{
    /// <summary>
    /// Represents the emotes to use in <see cref="Paginator"/>.
    /// </summary>
    /// <remarks>
    /// Set null for the emotes you want to disable.
    /// </remarks>
    public sealed class PaginatorReactions
    {
        public IEmote Front { get; set; } = new Emoji("⏮️");
        public IEmote Rear { get; set; } = new Emoji("⏭️");
        public IEmote Previous { get; set; } = new Emoji("◀️");
        public IEmote Next { get; set; } = new Emoji("▶️");
        public IEmote Stop { get; set; } = new Emoji("⏹️");
        public IEmote Trash { get; set; } = new Emoji("🗑");
        public IEmote Jump { get; set; } = new Emoji("🔢");
        public IEmote Info { get; set; } = new Emoji("ℹ️");
    }
}