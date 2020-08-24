using System;

namespace Discord.Addons.ShaneInteractive.Pagination
{
    public sealed class PaginatorJumpOptions
    {
        public bool DeleteResponse { get; set; } = true;
        public bool PromptEnabled { get; set; } = true;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(15);

        private string _prompt = "Enter page number to jump to.";

        /// <exception cref="ArgumentException">
        /// Thrown when value is null or whitespace.
        /// </exception>
        public string Prompt
        {
            get => _prompt;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Value cannot be null or empty", nameof(Prompt));

                _prompt = value;
            }
        }
    }
}