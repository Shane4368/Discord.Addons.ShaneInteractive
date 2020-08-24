using Discord;
using Discord.Addons.ShaneInteractive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace example
{
    public static class Program
    {
        private static readonly DiscordSocketClient _client = new DiscordSocketClient();
        private static readonly CommandService _commandService = new CommandService();
        private static IServiceProvider _services;

        private static ExampleCollectors exampleCollectors;
        private static ExamplePagination examplePagination;

        private static void Main() => MainAsync().GetAwaiter().GetResult();

        private static async Task MainAsync()
        {
            var token = File.ReadAllText("token.txt");

            exampleCollectors = new ExampleCollectors(_client);
            examplePagination = new ExamplePagination(_client);

            _services = new ServiceCollection()
                    .AddSingleton<InteractiveService>()
                    .BuildServiceProvider();

            _client.Log += OnLog;
            _client.MessageReceived += OnMessageReceived;

            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private static Task OnLog(LogMessage log)
        {
            Console.WriteLine(log);
            return Task.CompletedTask;
        }

        private static async Task OnMessageReceived(SocketMessage socketMessage)
        {
            if (socketMessage.Source != MessageSource.User) return;

            await HandleCommandAsync(socketMessage);
            await HandleRawCommandAsync(socketMessage);
        }

        private static async Task HandleCommandAsync(SocketMessage socketMessage)
        {
            var argPosition = 0;
            var message = (SocketUserMessage)socketMessage;

            if (!message.HasCharPrefix('!', ref argPosition)) return;

            var context = new SocketCommandContext(_client, message);

            await _commandService.ExecuteAsync(context, argPosition, _services);
        }

        private static async Task HandleRawCommandAsync(SocketMessage message)
        {
            if (message.Content == "--ptor")
                await examplePagination.Test_Paginator(message);
            else if (message.Content == "--ptor-embed")
                await examplePagination.Test_PaginatorWithEmbed(message);

            else if (message.Content == "--m-collector")
                await exampleCollectors.Test_MessageCollector(message);
            else if (message.Content == "--m-collector-events")
                await exampleCollectors.Test_MessageCollectorEvents(message);

            else if (message.Content == "--r-collector")
                await exampleCollectors.Test_ReactionCollector(message);
            else if (message.Content == "--r-collector-events")
                await exampleCollectors.Test_ReactionCollectorEvents(message);

            else if (message.Content == "--cls") Console.Clear();
        }
    }
}