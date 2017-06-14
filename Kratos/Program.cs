using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using CommandLine;
using Kratos.Configuration;
using Kratos.Services;
using Kratos.Data;

namespace Kratos
{
    class Program
    {
        static void Main(string[] args) =>
            new Program().MainAsync(args).GetAwaiter().GetResult();

        public static string GetConfigurationPath(string name) => Path.Combine(Directory.GetCurrentDirectory(), "config", name);
        public static string GetLogPath(string name) => Path.Combine(Directory.GetCurrentDirectory(), "log", name);

        private IServiceProvider _services;
        private DiscordSocketClient _client;
        private GuildConfiguration _guildsConfig;

        public async Task MainAsync(string[] args)
        {
            Console.Title = "Kratos";
            _services = ConfigureServices();

            CommandLineArguments options = null;

            var result = CommandLine.Parser.Default.ParseArguments<CommandLineArguments>(args)
                .WithParsed(o => options = o)
                .WithNotParsed(errors =>
                {
                    foreach (var e in errors)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    Console.WriteLine("Exiting...");
                    Environment.Exit(0);
                });

            Directory.CreateDirectory(GetConfigurationPath(""));
            Directory.CreateDirectory(GetLogPath(""));

            var botConfig = _services.GetService<BotConfiguration>();
            await botConfig.LoadAsync();
            _guildsConfig = _services.GetService<GuildConfiguration>();
            await _guildsConfig.LoadAsync();

            if (botConfig.Token == null && options.Token == null)
            {
                Console.WriteLine("No token found and none provided via command line arguments. Enter your token or leave blank to exit:");
                var token = Console.ReadLine();
                if (string.IsNullOrEmpty(token) || string.IsNullOrWhiteSpace(token)) Environment.Exit(0);
                botConfig.Token = token;
                await botConfig.SaveAsync();
            }
            else if ((botConfig.Token == null && options.Token != null) || (botConfig.Token != null && options.Token != null))
            {
                botConfig.Token = options.Token;
                await botConfig.SaveAsync();
            }

            _client = _services.GetService<DiscordSocketClient>();
            var localLog = _services.GetService<LocalLogService>();

            _client.Log += localLog.LogAsync;

            _client.Ready += HandleOfflineGuildChangesAsync;
            _client.LeftGuild += HandleOnlineGuildChangeAsync;

            var handler = new CommandHandler(_services);
            await handler.InstallCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, botConfig.Token);
            await _client.StartAsync();
            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices()
        {
            var collection = new ServiceCollection()
                .AddSingleton<BotConfiguration>()
                .AddSingleton<GuildConfiguration>()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Debug,
                    MessageCacheSize = 500 // Per channel
                }))
                .AddSingleton<LocalLogService>()
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    LogLevel = LogSeverity.Debug
                }));

            return collection.BuildServiceProvider();
        }

        private async Task HandleOfflineGuildChangesAsync()
        {
            _guildsConfig.Guilds.RemoveAll(g => !_client.Guilds.Any(x => x.Id == g.Id));
            await _guildsConfig.SaveAsync();
        }

        private async Task HandleOnlineGuildChangeAsync(SocketGuild g)
        {
            _guildsConfig.Guilds.RemoveAll(x => x.Id == g.Id);
            await _guildsConfig.SaveAsync();
        }
    }
}