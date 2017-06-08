using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Kratos.Configuration;
using Kratos.Services;

namespace Kratos
{
    class Program
    {
        static void Main(string[] args) =>
            new Program().MainAsync(args).GetAwaiter().GetResult();

        public static string GetConfigurationPath(string name) => Path.Combine(Directory.GetCurrentDirectory(), "config", name);

        private IServiceProvider _services;

        public async Task MainAsync(string[] args)
        {
            Console.Title = "Kratos";
            _services = ConfigureServices();

            Directory.CreateDirectory(GetConfigurationPath(""));

            var botConfig = _services.GetService<BotConfiguration>();
            await botConfig.LoadAsync();

            if (botConfig.Token == null && args.Length < 1)
            {
                Console.WriteLine("No token found and none provided via command line arguments. Enter your token or leave blank to exit:");
                var token = Console.ReadLine();
                if (string.IsNullOrEmpty(token) || string.IsNullOrWhiteSpace(token)) Environment.Exit(0);
                botConfig.Token = token;
                await botConfig.SaveAsync();
            }
            else if (botConfig.Token == null && args.Length >= 1)
            {
                var token = args[0];
                botConfig.Token = token;
                await botConfig.SaveAsync();
            }

            var client = _services.GetService<DiscordSocketClient>();
            var localLog = _services.GetService<LocalLogService>();

            client.Log += localLog.Log;
            await client.LoginAsync(TokenType.Bot, botConfig.Token);
            await client.StartAsync();
            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices()
        {
            var collection = new ServiceCollection()
                .AddSingleton<BotConfiguration>()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Debug,
                    MessageCacheSize = 500 // Per channel
                }))
                .AddSingleton<LocalLogService>();

            return collection.BuildServiceProvider();
        }
    }
}