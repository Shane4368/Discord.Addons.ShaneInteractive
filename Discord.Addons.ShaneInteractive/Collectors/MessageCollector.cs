using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.Addons.ShaneInteractive.Collectors
{
    public sealed class MessageCollector
    {
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="filter"/> or <paramref name="options"/> is null.
        /// </exception>
        public MessageCollector(Predicate<SocketMessage> filter, MessageCollectorConfig options)
        {
            _filter = filter ?? throw new ArgumentNullException(nameof(filter));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _options.Validate();
        }

        private readonly TaskCompletionSource<IEnumerable<SocketMessage>> _collectorCompletion =
            new TaskCompletionSource<IEnumerable<SocketMessage>>();

        private readonly List<SocketMessage> _messages = new List<SocketMessage>();
        private readonly Predicate<SocketMessage> _filter;
        private readonly MessageCollectorConfig _options;

        private TaskCompletionSource<bool> _isActiveCompletion = new TaskCompletionSource<bool>();
        private bool _canStart = true;
        private int _maxMessages = 0;

        /// <summary>
        /// Fired when a message passes the filter and is collected.
        /// </summary>
        public event Func<SocketMessage, Task> Collect;

        /// <summary>
        /// Fired when a collected message has been deleted.
        /// </summary>
        public event Func<SocketMessage, Task> Dispense;

        public event Func<IEnumerable<SocketMessage>, string, Task> End;

        /// <exception cref="InvalidOperationException">
        /// Thrown when collector has been previously used.
        /// </exception>
        public async Task<IEnumerable<SocketMessage>> StartAsync(bool returnCollected = true)
        {
            if (!_canStart) throw new InvalidOperationException("Collector cannot be reused");

            _canStart = false;

            _options.Client.MessageReceived += OnMessageReceived;
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
                : Enumerable.Empty<SocketMessage>();
        }

        public void Stop(string reason = "Stopped by user")
        {
            _collectorCompletion.SetResult(_messages);

            _options.Client.MessageReceived -= OnMessageReceived;
            _options.Client.MessageDeleted -= OnMessageDeleted;
            _options.Client.ChannelDestroyed -= OnChannelDestroyed;
            _options.Client.LeftGuild -= OnLeftGuild;

            End?.Invoke(_messages, reason);
        }

        private Task OnMessageReceived(SocketMessage socketMessage)
        {
            if (socketMessage.Channel.Id != _options.Channel.Id) return Task.CompletedTask;

            if (_options.ResetTimeout) _isActiveCompletion.SetResult(true);

            if (++_maxMessages == _options.MaxMessages) Stop("MaxMessages reached");

            if (!_filter(socketMessage)) return Task.CompletedTask;

            _messages.Add(socketMessage);
            Collect?.Invoke(socketMessage);

            if (_messages.Count == _options.Max) Stop("Max reached");

            return Task.CompletedTask;
        }

        private Task OnMessageDeleted(Cacheable<IMessage, ulong> cache, ISocketMessageChannel channel)
        {
            if (_options.Dispense && channel.Id == _options.Channel.Id)
            {
                var msg = _messages.Find(x => x.Id == cache.Id);

                if (msg != null)
                {
                    _messages.Remove(msg);
                    Dispense?.Invoke(msg);
                }
            }

            return Task.CompletedTask;
        }

        private Task OnChannelDestroyed(SocketChannel channel)
        {
            if (channel.Id == _options.Channel.Id) Stop("Channel was destroyed");
            return Task.CompletedTask;
        }

        private Task OnLeftGuild(SocketGuild guild)
        {
            if (_options.Channel is IGuildChannel channel && channel.GuildId == guild.Id)
                Stop("Left guild");

            return Task.CompletedTask;
        }
    }
}