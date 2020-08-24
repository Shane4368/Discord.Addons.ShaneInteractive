using Discord;
using Discord.Addons.ShaneInteractive.Collectors;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace example
{
    public sealed class ExampleCollectors
    {
        private readonly DiscordSocketClient _client;

        public ExampleCollectors(DiscordSocketClient client) => _client = client;

        public Task Test_MessageCollector(SocketMessage message)
        {
            var options = new MessageCollectorConfig(_client)
            {
                Channel = message.Channel,
                Timeout = TimeSpan.FromMinutes(1),
                Max = 3
            };

            var collector = new MessageCollector(x => !x.Author.IsBot, options);

            // To avoid blocking the gateway
            _ = Task.Run(async () =>
              {
                  try
                  {
                      var collected = await collector.StartAsync();
                      foreach (var msg in collected) Console.WriteLine(msg.Content);
                  }
                  catch (Exception exception)
                  {
                      Console.WriteLine(exception);
                  }
              });

            return Task.CompletedTask;
        }

        public async Task Test_MessageCollectorEvents(SocketMessage message)
        {
            var options = new MessageCollectorConfig(_client)
            {
                Channel = message.Channel,
                Timeout = TimeSpan.FromMinutes(1),
                Max = 3
            };

            var collector = new MessageCollector(x => !x.Author.IsBot, options);

            collector.Collect += (msg) =>
            {
                Console.WriteLine("MessageCollector#Collect: {0}", msg.Content);
                return Task.CompletedTask;
            };

            collector.Dispense += (msg) =>
            {
                Console.WriteLine("MessageCollector#Dispense: {0}", msg.Content);
                return Task.CompletedTask;
            };

            collector.End += (collectedMessages, reason) =>
            {
                Console.WriteLine("MessageCollector#End: {0}", reason);
                return Task.CompletedTask;
            };

            await collector.StartAsync(false);
        }

        public Task Test_ReactionCollector(SocketMessage message)
        {
            var options = new ReactionCollectorConfig(_client)
            {
                Message = message as IUserMessage,
                Timeout = TimeSpan.FromMinutes(1)
            };

            var collector = new ReactionCollector(x => true, options);

            // To avoid blocking the gateway
            _ = Task.Run(async () =>
            {
                try
                {
                    var collected = await collector.StartAsync();

                    foreach (var result in collected)
                    {
                        Console.WriteLine("{0} | {1}", result.Emote.Name, string.Join(", ", result.UserIds));
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            });

            return Task.CompletedTask;
        }

        public async Task Test_ReactionCollectorEvents(SocketMessage message)
        {
            var options = new ReactionCollectorConfig(_client)
            {
                Message = message as IUserMessage,
                Timeout = TimeSpan.FromMinutes(1)
            };

            var collector = new ReactionCollector(x => true, options);

            collector.Collect += (reaction) =>
            {
                Console.WriteLine("ReactionCollector#Collect: {0}", reaction.Emote.Name);
                return Task.CompletedTask;
            };

            collector.Dispense += (reaction) =>
            {
                Console.WriteLine("ReactionCollector#Dispense: {0}", reaction.Emote.Name);
                return Task.CompletedTask;
            };

            collector.End += (collected, reason) =>
            {
                Console.WriteLine("ReactionCollector#End: Reason: {0}", reason);

                foreach (var result in collected)
                {
                    Console.WriteLine("{0} | {1}", result.Emote.Name, string.Join(", ", result.UserIds));
                }

                return Task.CompletedTask;
            };

            await collector.StartAsync(false);
        }
    }
}