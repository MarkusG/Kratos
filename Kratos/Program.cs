using System;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
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

        public const string Version = "b1.2.3";
        public static string ConfigDirectory = Directory.GetCurrentDirectory() + @"/config/";
        #region Private fields
        private DiscordSocketClient _client;
        private BlacklistService _blacklist;
        private PermissionsService _permissions;
        private UsernoteService _usernotes;
        private RecordService _records;
        private UnpunishService _unpunish;
        private LogService _log;
        private SlowmodeService _slowmode;
        private RatelimitService _ratelimit;
        private CommandHandler _commands;
        private DependencyMap _map;
        private CoreConfig _config;
        #endregion
        public async Task Start()
        {
            Console.Title = $"Kratos {Version}";
            #region Setting up DiscordClient
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 100
            });

            _client.Log += Log;
            #endregion
            #region Setting up configs
            if (!Directory.Exists(ConfigDirectory))
                Directory.CreateDirectory(ConfigDirectory);

            if (File.Exists(ConfigDirectory + @"core.json"))
                _config = await CoreConfig.UseCurrentAsync();
            else
                _config = await CoreConfig.CreateNewAsync();
            #endregion
            #region Setting up services
            _usernotes = new UsernoteService();

            _records = new RecordService();

            _log = new LogService(_client);
            await _log.LoadConfigurationAsync();

            _slowmode = new SlowmodeService(_client, _log, _config);

            _unpunish = new UnpunishService(_client, _blacklist, _log, _records, _config);
            await _unpunish.GetRecordsAsync();

            _ratelimit = new RatelimitService(_client, _config, _records, _unpunish, _log);
            await _ratelimit.LoadConfigurationAsync();
            if (_ratelimit.IsEnabled)
                _ratelimit.Enable(_ratelimit.Limit);

            _blacklist = new BlacklistService(_client, _unpunish, _records, _log, _config);
            await _blacklist.LoadConfigurationAsync();
            if (_blacklist.IsEnabled)
                _blacklist.Enable();

            _permissions = new PermissionsService();
            _permissions.AddPermissions(Assembly.GetEntryAssembly());
            await _permissions.LoadConfigurationAsync();
            #endregion
            #region Adding dependencies to map
            _map = new DependencyMap();
            _map.Add(_blacklist);
            _map.Add(_permissions);
            _map.Add(_usernotes);
            _map.Add(_records);
            _map.Add(_unpunish);
            _map.Add(_log);
            _map.Add(_slowmode);
            _map.Add(_ratelimit);
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
            #region Start unpunisher loop
            await _unpunish.StartAsync();
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

            Console.WriteLine(m.ToString());
            if (m.Exception != null)
                Console.WriteLine(m.Exception);
            Console.ForegroundColor = ConsoleColor.Gray;

            return Task.CompletedTask;
        }
    }
}
