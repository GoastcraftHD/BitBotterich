using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BitBotterich;
using BitBotterich.util;

public class Program
{
    private static IConfiguration _config;
    private static IServiceProvider _services;

    private static readonly DiscordSocketConfig _socketConfig = new()
    {
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers,
        AlwaysDownloadUsers = true,
        UseInteractionSnowflakeDate = false,
    };

    public static async Task Main(string[] args)
    {
        if (!File.Exists("./config.json"))
        {
            string configContent =
                    @"{
                        ""TOKEN"": """"
                    }
                    ";
            File.WriteAllText("./config.json", configContent);

            Console.WriteLine("Please add a Token in the config.json");
            return;
        }

        if (File.Exists("./Quotes.json"))
        {
            QuoteHelper.LoadQuotes();
        }

        _config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(path: "config.json")
            .Build();

        _services = new ServiceCollection()
            .AddSingleton(_config)
            .AddSingleton(_socketConfig)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<InteractionHandler>()
            .BuildServiceProvider();

        var client = _services.GetRequiredService<DiscordSocketClient>();

        client.Log += LogAsync;

        await _services.GetRequiredService<InteractionHandler>()
            .InitializeAsync();

        await client.LoginAsync(TokenType.Bot, _config["Token"]);
        await client.StartAsync();

        await Task.Delay(Timeout.Infinite);
    }

    private static async Task LogAsync(LogMessage message)
        => Console.WriteLine(message.ToString());
}
