using Discord;
using Discord.Addons.ShaneInteractive.Pagination;
using Discord.WebSocket;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace example
{
    public sealed class ExamplePagination
    {
        private static readonly string _loremIpsum = File.ReadAllText("lorem-ipsum.txt");

        private readonly DiscordSocketClient _client;

        public ExamplePagination(DiscordSocketClient client) => _client = client;

        public async Task Test_Paginator(SocketMessage message)
        {
            var pages = new[] { "Page 1", "Page 2", "Page 3", "Page 4" };

            var paginatorBuilder = new PaginatorBuilder(_client)
            {
                Timeout = TimeSpan.FromMinutes(1),
                Pages = pages.Select(x => $"{x}\n{_loremIpsum}\n\nPage {{0}}/{{1}}"),
                Filter = (x) => x.UserId == message.Author.Id,
                Reactions = new PaginatorReactions
                {
                    Front = null,
                    Rear = null,
                    Trash = null,
                    Previous = null,
                    Next = null
                }
            };

            await new Paginator(paginatorBuilder).StartAsync(message.Channel);
        }

        public async Task Test_PaginatorWithEmbed(SocketMessage message)
        {
            var pages = new[] { "Page 1", "Page 2", "Page 3", "Page 4" };

            var paginatorBuilder = new PaginatorBuilder(_client)
            {
                CircularEnabled = false,
                Timeout = TimeSpan.FromMinutes(1),
                Pages = pages.Select(x => new EmbedBuilder { Description = $"{x}\n{_loremIpsum}" }.Build()),
                Filter = (x) => x.UserId == message.Author.Id,
                Reactions = new PaginatorReactions
                {
                    Front = null,
                    Rear = null,
                    Trash = null,
                    Jump = null,
                    Stop = null,
                    Info = Emote.Parse("<:yay:523590001120903172>")
                },
                Content = "This is just normal text that will always be displayed.",
                EmbedTemplate = new EmbedBuilder()
                    .WithTitle("Constant Title")
                    .WithAuthor("Constant Author Name", message.Author.GetAvatarUrl())
                    .WithColor(Color.Blue)
                    .WithThumbnailUrl(message.Author.GetAvatarUrl())
                    .WithFooter("This is page {0} of {1}")
                    .WithCurrentTimestamp()
                    .Build()
            };

            await new CustomPaginator(paginatorBuilder).StartAsync(message.Channel);
        }
    }
}