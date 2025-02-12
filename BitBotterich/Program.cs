﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BitBotterich;
using BitBotterich.util;

public class Program
{
    public static IConfiguration? Config;

    private static IServiceProvider? _services;

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
            // For non docker enviorments
            File.Copy("../../../config.json", "./config.json");
        }

        if (File.Exists("./data/quotes.json"))
        {
            QuoteHelper.LoadQuotes();
        }

        Config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(path: "config.json")
            .Build();

        if (Config is null)
        {
            Console.WriteLine("Could not create the config");
            return;
        }

        _services = new ServiceCollection()
            .AddSingleton(Config)
            .AddSingleton(_socketConfig)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<InteractionHandler>()
            .BuildServiceProvider();

        var client = _services.GetRequiredService<DiscordSocketClient>();

        client.Log += LogAsync;

        await _services.GetRequiredService<InteractionHandler>()
            .InitializeAsync();

        string? botToken = Config["Token"];

        if (botToken is null)
        {
            Console.WriteLine("Could not retrieve Discord credentials!");
            return;
        }

        await client.LoginAsync(TokenType.Bot, botToken);
        await client.StartAsync();

        string? spotifyClientID = Config["SpotifyClientID"];
        string? spotifyClientSecret = Config["SpotifyClientSecret"];

        if (string.IsNullOrEmpty(spotifyClientID) || string.IsNullOrEmpty(spotifyClientSecret))
        {
            Console.WriteLine("Could not retrieve Spotify credentials!");
            return;
        }

        SpotifyUtil.ClientID = spotifyClientID;
        SpotifyUtil.ClientSecret = spotifyClientSecret;

        await SpotifyUtil.GetTokenAsync();

        await Task.Delay(Timeout.Infinite);
    }

    private static async Task LogAsync(LogMessage message)
    {
        Console.WriteLine(message.ToString());
        await Task.CompletedTask;
    }
}
