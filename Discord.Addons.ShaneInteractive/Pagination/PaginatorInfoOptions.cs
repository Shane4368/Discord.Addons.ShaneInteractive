using System;

namespace Discord.Addons.ShaneInteractive.Pagination
{
    public sealed class PaginatorInfoOptions
    {
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

        private string _text = "This is a paginator. React to change page.";

        /// <exception cref="ArgumentException">
        /// Thrown when value is null or whitespace.
        /// </exception>
        public string Text
        {
            get => _text;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Value cannot be null or empty", nameof(Text));

                _text = value;
            }
        }
    }
}