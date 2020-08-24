using Discord.WebSocket;
using System;
using System.Collections.Generic;

namespace Discord.Addons.ShaneInteractive.Pagination
{
    public sealed class PaginatorBuilder
    {
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="client"/> is null.
        /// </exception>
        public PaginatorBuilder(BaseSocketClient client)
            => Client = client ?? throw new ArgumentNullException(nameof(client));

        internal BaseSocketClient Client { get; }

        public PaginatorInfoOptions InfoOptions { get; set; } = new PaginatorInfoOptions();
        public PaginatorJumpOptions JumpOptions { get; set; } = new PaginatorJumpOptions();
        public PaginatorReactions Reactions { get; set; } = new PaginatorReactions();
        public TimeSpan? Timeout { get; set; } = TimeSpan.FromMinutes(15);
        public bool CircularEnabled { get; set; } = true;

        /// <summary>
        /// <para>Embed to sync across all pages.</para>
        /// <para>The properties on this embed takes precedence.</para>
        /// </summary>
        public Embed EmbedTemplate { get; set; }
        /// <summary>
        /// The text to be displayed when using embed pages.
        /// </summary>
        public string Content { get; set; }
        public bool DeleteOnTimeout { get; set; }

        public Predicate<SocketReaction> Filter { get; set; }
        public IEnumerable<object> Pages { get; set; }

        public PaginatorBuilder WithReactions(PaginatorReactions reactions)
        {
            Reactions = reactions;
            return this;
        }

        public PaginatorBuilder WithInfoOptions(PaginatorInfoOptions infoOptions)
        {
            InfoOptions = infoOptions;
            return this;
        }

        public PaginatorBuilder WithInfoOptions(string text = null, TimeSpan? timeout = null)
        {
            var infoOptions = new PaginatorInfoOptions();

            infoOptions.Text = text ?? infoOptions.Text;
            infoOptions.Timeout = timeout ?? infoOptions.Timeout;

            return WithInfoOptions(infoOptions);
        }

        public PaginatorBuilder WithJumpOptions(PaginatorJumpOptions jumpOptions)
        {
            JumpOptions = jumpOptions;
            return this;
        }

        public PaginatorBuilder WithJumpOptions(string prompt = null, bool promptEnabled = true, bool deleteResponse = true, TimeSpan? timeout = null)
        {
            var jumpOptions = new PaginatorJumpOptions
            {
                DeleteResponse = deleteResponse,
                PromptEnabled = promptEnabled
            };

            jumpOptions.Prompt = prompt ?? jumpOptions.Prompt;
            jumpOptions.Timeout = timeout ?? jumpOptions.Timeout;

            return WithJumpOptions(jumpOptions);
        }

        public PaginatorBuilder WithPages(IEnumerable<string> pages)
        {
            Pages = pages;
            return this;
        }

        public PaginatorBuilder WithPages(IEnumerable<Embed> pages)
        {
            Pages = pages;
            return this;
        }

        public PaginatorBuilder WithTimeout(TimeSpan? timeout)
        {
            Timeout = timeout;
            return this;
        }

        public PaginatorBuilder WithDeleteOnTimeout(bool deleteOnTimeout)
        {
            DeleteOnTimeout = deleteOnTimeout;
            return this;
        }

        public PaginatorBuilder WithCircularEnabled(bool circularEnabled)
        {
            CircularEnabled = circularEnabled;
            return this;
        }

        public PaginatorBuilder WithEmbedTemplate(Embed embedTemplate)
        {
            EmbedTemplate = embedTemplate;
            return this;
        }

        public PaginatorBuilder WithContent(string content)
        {
            Content = content;
            return this;
        }

        public PaginatorBuilder WithFilter(Predicate<SocketReaction> filter)
        {
            Filter = filter;
            return this;
        }

        internal void Validate()
        {
            if (InfoOptions == null) throw new ArgumentNullException(nameof(InfoOptions));
            if (JumpOptions == null) throw new ArgumentNullException(nameof(JumpOptions));
            if (Reactions == null) throw new ArgumentNullException(nameof(Reactions));
            if (Filter == null) throw new ArgumentNullException(nameof(Filter));
            if (Pages == null) throw new ArgumentNullException(nameof(Pages));
        }
    }
}