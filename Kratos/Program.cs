using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using CommandLine;
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

            var botConfig = _services.GetService<BotConfiguration>();
            await botConfig.LoadAsync();

            if (botConfig.Token == null && options.Token == null)
            {
                Console.WriteLine("No token found and none provided via command line arguments. Enter your token or leave blank to exit:");
                var token = Console.ReadLine();
                if (string.IsNullOrEmpty(token) || string.IsNullOrWhiteSpace(token)) Environment.Exit(0);
                botConfig.Token = token;
                await botConfig.SaveAsync();
            }
            else if (botConfig.Token == null && options.Token != null)
            {
                botConfig.Token = options.Token;
                await botConfig.SaveAsync();
            }

            var client = _services.GetService<DiscordSocketClient>();
            var localLog = _services.GetService<LocalLogService>();

            localLog.LogToFile = options.LogToFile;
            client.Log += localLog.Log;

            var handler = new CommandHandler(_services);
            await handler.InstallCommandsAsync();

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
                .AddSingleton<LocalLogService>()
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    LogLevel = LogSeverity.Debug
                }));

            return collection.BuildServiceProvider();
        }
    }
}