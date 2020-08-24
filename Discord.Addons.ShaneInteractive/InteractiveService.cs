using Discord.Addons.ShaneInteractive.Collectors;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.Addons.ShaneInteractive
{
    public class InteractiveService
    {
        private readonly TimeSpan _defaultTimeout;

        public InteractiveService(InteractiveServiceConfig config = null)
            => _defaultTimeout = (config ?? new InteractiveServiceConfig()).DefaultTimeout;

        public async Task<SocketMessage> NextMessageAsync(SocketCommandContext context, TimeSpan? timeout)
        {
            var options = new MessageCollectorConfig(context.Client)
            {
                Channel = context.Channel,
                Timeout = timeout ?? _defaultTimeout,
                Max = 1
            };

            var collected = await new MessageCollector(x => x.Author.Id == context.User.Id, options)
                .StartAsync()
                .ConfigureAwait(false);

            return collected.FirstOrDefault();
        }

        public async Task<IUserMessage> ReplyAndDeleteAsync(SocketCommandContext context, string text, bool isTTS, Embed embed, TimeSpan? timeout, RequestOptions options)
        {
            timeout = timeout ?? _defaultTimeout;

            var message = await context.Channel.SendMessageAsync(text, isTTS, embed, options).ConfigureAwait(false);

            await Task.Delay(timeout.Value).ConfigureAwait(false);
            await message.DeleteAsync().ConfigureAwait(false);

            return message;
        }
    }
}