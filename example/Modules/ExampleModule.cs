using Discord.Addons.ShaneInteractive;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace example.Modules
{
    public sealed class ExampleModule : InteractiveBase<SocketCommandContext>
    {
        [Command("next-msg", RunMode = RunMode.Async)]
        public async Task Test_NextMessageAsync()
        {
            await Context.Channel.SendMessageAsync("Testing: NextMessageAsync");

            var response = await NextMessageAsync();

            if (response != null)
            {
                await ReplyAsync($"Testing: Your response was:\n>>> {response.Content}");
            }
        }

        [Command("reply-n-del", RunMode = RunMode.Async)]
        public async Task Test_ReplyAndDeleteAsync()
        {
            var deleted = await ReplyAndDeleteAsync("Testing: ReplyAndDeleteAsync");
            Console.WriteLine(deleted.Content);
        }

        [RequireOwner]
        [Command("logout")]
        public Task LogoutAsync()
        {
            Context.Client.LogoutAsync().ContinueWith(_ => Environment.Exit(0));
            return Task.CompletedTask;
        }
    }
}