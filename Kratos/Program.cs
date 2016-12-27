using System;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Linq;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Kratos.Services;
using Kratos.Configs;

namespace Kratos
{
    class Program
    {
        static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();

        public const string Version = "b1.0";

        private DiscordSocketClient _client;
        private BlacklistService _blacklist;
        private PermissionsService _permissions;
        private UsernoteService _usernotes;
        private CommandHandler _commands;
        private DependencyMap _map;
        private CoreConfig _config;

        public async Task Start()
        {
            Console.Title = $"Kratos {Version}";
            #region Setting up DiscordClient
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Info,
                MessageCacheSize = 0
            });

            _client.Log += Log;
            #endregion
            #region Setting up configs
            if (!Directory.Exists("config"))
                Directory.CreateDirectory("config");

            if (File.Exists(@"config\core.json"))
                _config = await CoreConfig.UseCurrent();
            else
                _config = await CoreConfig.CreateNew();
            #endregion
            #region Setting up services
            _blacklist = new BlacklistService(_client);
            await _blacklist.LoadConfigurationAsync();
            _blacklist.Enable();

            _permissions = new PermissionsService();
            var permissions = Assembly.GetEntryAssembly().GetTypes()
                          .SelectMany(x => x.GetMethods())
                          .Where(x => x.GetCustomAttributes<Preconditions.RequireCustomPermissionAttribute>().Count() > 0)
                          .Select(x => x.GetCustomAttribute<Preconditions.RequireCustomPermissionAttribute>().Permission);
            foreach (var p in permissions)
            {
                if (!_permissions.AllPermissions.Contains(p))
                    _permissions.AllPermissions.Add(p);
            }

            await _permissions.LoadConfigurationAsync();

            _usernotes = new UsernoteService();
            #endregion
            #region Adding dependencies to map
            _map = new DependencyMap();
            _map.Add(_blacklist);
            _map.Add(_permissions);
            _map.Add(_usernotes);
            _map.Add(_client);
            _map.Add(_config);
            #endregion
            #region Setting up commands
            _commands = new CommandHandler(_map);
            await _commands.InstallAsync();
            #endregion
            #region Connect to Discord
            await _client.LoginAsync(TokenType.Bot, _config.Token);
            await _client.ConnectAsync();
            #endregion
            await Task.Delay(-1);
        }

        private Task Log(LogMessage m)
        {
            switch (m.Severity)
            {
                case LogSeverity.Warning: Console.ForegroundColor = ConsoleColor.Yellow; break;
                case LogSeverity.Error: Console.ForegroundColor = ConsoleColor.Red; break;
                case LogSeverity.Critical: Console.ForegroundColor = ConsoleColor.DarkRed; break;
            }

            Console.WriteLine($"[{DateTime.Now}] [{m.Severity}] [{m.Source}] {m.Message}");
            if (m.Exception != null)
                Console.WriteLine(m.Exception);
            Console.ForegroundColor = ConsoleColor.Gray;

            return Task.CompletedTask;
        }
    }
}
