using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Discord.Addons.ShaneInteractive
{
    public abstract class InteractiveBase<T> : ModuleBase<T> where T : SocketCommandContext
    {
        public InteractiveService Interactive { get; set; }

        public Task<SocketMessage> NextMessageAsync(TimeSpan? timeout = null)
            => Interactive.NextMessageAsync(Context, timeout);

        public Task<IUserMessage> ReplyAndDeleteAsync(string text = null, bool isTTS = false, Embed embed = null, TimeSpan? timeout = null, RequestOptions options = null)
            => Interactive.ReplyAndDeleteAsync(Context, text, isTTS, embed, timeout, options);
    }
}