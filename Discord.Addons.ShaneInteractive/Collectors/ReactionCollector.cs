using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.Addons.ShaneInteractive.Collectors
{
    public sealed class ReactionCollector
    {
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="filter"/> or <paramref name="options"/> is null.
        /// </exception>
        public ReactionCollector(Predicate<SocketReaction> filter, ReactionCollectorConfig options)
        {
            _filter = filter ?? throw new ArgumentNullException(nameof(filter));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _options.Validate();
        }

        private readonly TaskCompletionSource<IEnumerable<ReactionCollectorResult>> _collectorCompletion =
            new TaskCompletionSource<IEnumerable<ReactionCollectorResult>>();

        private readonly List<SocketReaction> _reactions = new List<SocketReaction>();
        private readonly HashSet<IEmote> _emotes = new HashSet<IEmote>();
        private readonly HashSet<ulong> _users = new HashSet<ulong>();
        private readonly Predicate<SocketReaction> _filter;
        private readonly ReactionCollectorConfig _options;

        private TaskCompletionSource<bool> _isActiveCompletion = new TaskCompletionSource<bool>();
        private bool _canStart = true;

        /// <summary>
        /// Fired when a reaction passes the filter and is collected.
        /// </summary>
        public event Func<SocketReaction, Task> Collect;

        /// <summary>
        /// Fired when a collected reaction has been removed.
        /// </summary>
        public event Func<SocketReaction, Task> Dispense;

        public event Func<IEnumerable<ReactionCollectorResult>, string, Task> End;

        /// <exception cref="InvalidOperationException">
        /// Thrown when collector has been previously used.
        /// </exception>
        public async Task<IEnumerable<ReactionCollectorResult>> StartAsync(bool returnCollected = true)
        {
            if (!_canStart) throw new InvalidOperationException("Collector cannot be reused");

            _canStart = false;

            _options.Client.ReactionAdded += OnReactionAdded;
            _options.Client.ReactionRemoved += OnReactionRemoved;
            _options.Client.MessageDeleted += OnMessageDeleted;
            _options.Client.ChannelDestroyed += OnChannelDestroyed;
            _options.Client.LeftGuild += OnLeftGuild;

            var collectorTrigger = _collectorCompletion.Task;

            if (_options.Timeout.HasValue)
            {
                // I am so sorry for implementing "fire and forget," but it was the only solution I could think of.
                _ = Task.Run(async () =>
                {
                    while (true)
                    {
                        var isActiveTrigger = _isActiveCompletion.Task;
                        var delayTrigger = Task.Delay(_options.Timeout.Value);

                        var task = await Task
                            .WhenAny(collectorTrigger, delayTrigger, isActiveTrigger)
                            .ConfigureAwait(false);

                        if (task == isActiveTrigger)
                        {
                            _isActiveCompletion = new TaskCompletionSource<bool>();
                        }
                        else
                        {
                            if (task == delayTrigger) Stop("Timed out");
                            break;
                        }
                    }
                });
            }

            return returnCollected
                ? await collectorTrigger.ConfigureAwait(false)
                : Enumerable.Empty<ReactionCollectorResult>();
        }

        public void Stop(string reason = "Stopped by user")
        {
            var collectorCompletionResult = GetResult();

            _collectorCompletion.SetResult(collectorCompletionResult);

            _options.Client.ReactionAdded -= OnReactionAdded;
            _options.Client.ReactionRemoved -= OnReactionRemoved;
            _options.Client.MessageDeleted -= OnMessageDeleted;
            _options.Client.ChannelDestroyed -= OnChannelDestroyed;
            _options.Client.LeftGuild -= OnLeftGuild;

            End?.Invoke(collectorCompletionResult, reason);
        }

        private IEnumerable<ReactionCollectorResult> GetResult()
        {
            var results = new HashSet<ReactionCollectorResult>();

            foreach (var reaction in _reactions)
            {
                var result = new ReactionCollectorResult(reaction.Emote);

                if (results.Add(result))
                {
                    result.UserIds = _reactions
                        .Where(x => x.Emote.Equals(reaction.Emote))
                        .Select(x => x.UserId)
                        .ToArray();
                }
            }

            return results;
        }

        private Task OnReactionAdded(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (cache.Id != _options.Message.Id) return Task.CompletedTask;

            if (_options.ResetTimeout) _isActiveCompletion.SetResult(true);

            if (!_filter(reaction)) return Task.CompletedTask;

            _reactions.Add(reaction);
            _emotes.Add(reaction.Emote);
            _users.Add(reaction.UserId);

            Collect?.Invoke(reaction);

            if (_reactions.Count == _options.Max) Stop("Max reached");
            else if (_emotes.Count == _options.MaxEmotes) Stop("MaxEmotes reached");
            else if (_users.Count == _options.MaxUsers) Stop("MaxUsers reached");

            return Task.CompletedTask;
        }

        private Task OnReactionRemoved(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (_options.Dispense && cache.Id == _options.Message.Id)
            {
                _reactions.Remove(reaction);
                Dispense?.Invoke(reaction);
            }

            return Task.CompletedTask;
        }

        private Task OnMessageDeleted(Cacheable<IMessage, ulong> cache, ISocketMessageChannel channel)
        {
            if (cache.Id == _options.Message.Id) Stop("Message was deleted");
            return Task.CompletedTask;
        }

        private Task OnChannelDestroyed(SocketChannel channel)
        {
            if (channel.Id == _options.Message.Channel.Id) Stop("Channel was destroyed.");
            return Task.CompletedTask;
        }

        private Task OnLeftGuild(SocketGuild guild)
        {
            if (_options.Message.Author is IGuildUser user && user.GuildId == guild.Id)
                Stop("Left guild");

            return Task.CompletedTask;
        }
    }
}