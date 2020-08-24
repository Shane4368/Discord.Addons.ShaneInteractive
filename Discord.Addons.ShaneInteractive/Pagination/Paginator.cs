using Discord.Addons.ShaneInteractive.Collectors;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.Addons.ShaneInteractive.Pagination
{
    public class Paginator
    {
        /// <exception cref="ArgumentException">
        /// Thrown when paginator has no pages.
        /// </exception>
        public Paginator(PaginatorBuilder builder)
        {
            builder.Validate();

            _pages = builder.Pages.ToArray();
            _pageCount = _pages.Length;

            if (_pageCount == 0)
                throw new ArgumentException("Paginator must have at least one page.", nameof(_pages));

            _lastPageIndex = _pageCount - 1;

            _timeout = builder.Timeout;
            _deleteOnTimeout = builder.DeleteOnTimeout;
            _circularEnabled = builder.CircularEnabled;
            _embedTemplate = builder.EmbedTemplate;
            _content = builder.Content;

            _infoOptions = builder.InfoOptions;
            _jumpOptions = builder.JumpOptions;
            _client = builder.Client;
            _filter = builder.Filter;

            Reactions = builder.Reactions;
        }

        private readonly BaseSocketClient _client;
        private readonly PaginatorInfoOptions _infoOptions;
        private readonly PaginatorJumpOptions _jumpOptions;
        private readonly Predicate<SocketReaction> _filter;
        private readonly object[] _pages;

        private readonly TimeSpan? _timeout;
        private readonly Embed _embedTemplate;
        private readonly string _content;
        private readonly bool _circularEnabled;
        private readonly bool _deleteOnTimeout;

        private readonly int _pageCount;
        private readonly int _lastPageIndex;

        private int _currentPageIndex = 0;

        private ReactionCollector _collector;
        private IUserMessage _message;

        protected PaginatorReactions Reactions { get; }

        public async Task StartAsync(IMessageChannel channel)
        {
            var firstPage = _pages[0];

            if (firstPage is string page)
            {
                _message = await channel
                    .SendMessageAsync(RenderPageNumber(page))
                    .ConfigureAwait(false);
            }
            else
            {
                _message = await channel
                    .SendMessageAsync(_content, embed: MergeEmbedTemplate((Embed)firstPage))
                    .ConfigureAwait(false);
            }

            // Don't bother adding reactions if there's only one page.
            if (_pageCount == 0) return;

            await AddReactionsAsync().ConfigureAwait(false);

            var options = new ReactionCollectorConfig(_client)
            {
                Message = _message,
                Timeout = _timeout
            };

            _collector = new ReactionCollector(x => _filter(x), options);
            _collector.Collect += OnCollect;

            if (_deleteOnTimeout)
            {
                _collector.End += (_, __) => _message.DeleteAsync();
            }

            await _collector.StartAsync(false).ConfigureAwait(false);
        }

        /// <summary>
        /// Override this method to change the order of reactions.
        /// </summary>
        protected virtual async Task AddReactionsAsync()
        {
            await AddReactionAsync(Reactions.Front).ConfigureAwait(false);
            await AddReactionAsync(Reactions.Previous).ConfigureAwait(false);
            await AddReactionAsync(Reactions.Next).ConfigureAwait(false);
            await AddReactionAsync(Reactions.Rear).ConfigureAwait(false);
            await AddReactionAsync(Reactions.Stop).ConfigureAwait(false);
            await AddReactionAsync(Reactions.Trash).ConfigureAwait(false);
            await AddReactionAsync(Reactions.Jump).ConfigureAwait(false);
            await AddReactionAsync(Reactions.Info).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a reaction to the message if the emote is not null.
        /// </summary>
        protected async Task AddReactionAsync(IEmote emote)
        {
            if (emote != null) await _message.AddReactionAsync(emote).ConfigureAwait(false);
        }

        private string RenderPageNumber(string text)
        {
            return text
                .Replace("{0}", (_currentPageIndex + 1).ToString())
                .Replace("{1}", _pageCount.ToString());
        }

        private Embed MergeEmbedTemplate(Embed embed)
        {
            var builder = embed.ToEmbedBuilder();

            if (_embedTemplate != null)
            {
                if (_embedTemplate.Color.HasValue)
                    builder.Color = _embedTemplate.Color;

                if (_embedTemplate.Thumbnail.HasValue)
                    builder.ThumbnailUrl = _embedTemplate.Thumbnail.Value.Url;

                if (_embedTemplate.Timestamp.HasValue)
                    builder.Timestamp = _embedTemplate.Timestamp;

                if (_embedTemplate.Title != null)
                    builder.Title = _embedTemplate.Title;

                if (_embedTemplate.Url != null)
                    builder.Url = _embedTemplate.Url;

                if (_embedTemplate.Description != null)
                    builder.Description = _embedTemplate.Description;

                if (_embedTemplate.Fields.Length > 0)
                {
                    builder.Fields.Clear();

                    foreach (var field in _embedTemplate.Fields)
                        builder.AddField(field.Name, field.Value, field.Inline);
                }

                if (_embedTemplate.Author.HasValue)
                {
                    var author = _embedTemplate.Author.Value;
                    builder.WithAuthor(author.Name, author.IconUrl, author.Url);
                }

                if (_embedTemplate.Footer.HasValue)
                {
                    var footer = _embedTemplate.Footer.Value;
                    builder.WithFooter(footer.Text, footer.IconUrl);
                }
            }

            var footerText = builder.Footer.Text ?? "Page {0}/{1}";

            builder.Footer.Text = RenderPageNumber(footerText);

            return builder.Build();
        }

        private async Task EditMessageAsync()
        {
            var currentPage = _pages[_currentPageIndex];

            if (currentPage is string page)
            {
                await _message
                    .ModifyAsync(x => x.Content = RenderPageNumber(page))
                    .ConfigureAwait(false);
            }
            else
            {
                await _message
                    .ModifyAsync(x => x.Embed = MergeEmbedTemplate((Embed)currentPage))
                    .ConfigureAwait(false);
            }
        }

        private async Task OnCollect(SocketReaction reaction)
        {
            var emote = reaction.Emote;
            var hasManageMessages = false;

            if (reaction.Channel is SocketTextChannel textChannel)
            {
                hasManageMessages = textChannel.Guild.CurrentUser.GetPermissions(textChannel).ManageMessages;

                if (hasManageMessages)
                    await _message.RemoveReactionAsync(emote, reaction.UserId).ConfigureAwait(false);
            }

            if (emote.Equals(Reactions.Front))
            {
                if (_currentPageIndex == 0) return;
                _currentPageIndex = 0;
            }
            else if (emote.Equals(Reactions.Rear))
            {
                if (_currentPageIndex == _lastPageIndex) return;
                _currentPageIndex = _lastPageIndex;
            }
            else if (emote.Equals(Reactions.Previous))
            {
                if (_circularEnabled)
                {
                    if (_currentPageIndex == 0) _currentPageIndex = _lastPageIndex;
                    else _currentPageIndex--;
                }
                else
                {
                    if (_currentPageIndex == 0) return;
                    _currentPageIndex--;
                }
            }
            else if (emote.Equals(Reactions.Next))
            {
                if (_circularEnabled)
                {
                    if (_currentPageIndex == _lastPageIndex) _currentPageIndex = 0;
                    else _currentPageIndex++;
                }
                else
                {
                    if (_currentPageIndex == _lastPageIndex) return;
                    _currentPageIndex++;
                }
            }
            else if (emote.Equals(Reactions.Stop))
            {
                _collector.Stop();
                return;
            }
            else if (emote.Equals(Reactions.Trash))
            {
                _collector.Stop();
                await _message.DeleteAsync().ConfigureAwait(false);
                return;
            }
            else if (emote.Equals(Reactions.Jump))
            {
                var promptMessage = _jumpOptions.PromptEnabled
                    ? await _message.Channel.SendMessageAsync(_jumpOptions.Prompt).ConfigureAwait(false)
                    : null;

                var collectorConfig = new MessageCollectorConfig(_client)
                {
                    Channel = _message.Channel,
                    Timeout = _jumpOptions.Timeout,
                    Max = 1
                };

                var messageCollector = new MessageCollector(
                    x => x.Author.Id == reaction.UserId, collectorConfig);

                var collected = await messageCollector.StartAsync().ConfigureAwait(false);
                var collectedMessage = collected.FirstOrDefault();

                if (promptMessage != null) await promptMessage.DeleteAsync().ConfigureAwait(false);

                if (collectedMessage != null)
                {
                    if (_jumpOptions.DeleteResponse && hasManageMessages)
                        await collectedMessage.DeleteAsync().ConfigureAwait(false);

                    if (int.TryParse(collectedMessage.Content, out var pageNumber) &&
                        pageNumber >= 1 && pageNumber <= _pageCount)
                        _currentPageIndex = pageNumber - 1;
                }
            }
            else if (emote.Equals(Reactions.Info))
            {
                var message = await _message.Channel.SendMessageAsync(_infoOptions.Text).ConfigureAwait(false);
                await Task.Delay(_infoOptions.Timeout).ConfigureAwait(false);
                await message.DeleteAsync().ConfigureAwait(false);
                return;
            }

            await EditMessageAsync().ConfigureAwait(false);
        }
    }
}