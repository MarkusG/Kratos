using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Discord;
using Discord.WebSocket;
using Kratos.Configs;

namespace Kratos.Services
{
    public class BlacklistService
    {
        private DiscordSocketClient _client;
        private UnpunishService _unpunish;
        private LogService _log;
        private RecordService _records;
        private CoreConfig _config;

        public List<Regex> Blacklist { get; set; }
        public TimeSpan MuteTime { get; set; }
        public bool IsEnabled { get; private set; }

        public void Enable()
        {
            _client.MessageReceived += _client_MessageReceived_Blacklist;
            IsEnabled = true;
        }
        public void Disable()
        {
            _client.MessageReceived -= _client_MessageReceived_Blacklist;
            IsEnabled = false;
        }

        public async Task<bool> SaveConfigurationAsync()
        {
            var config = new BlacklistConfig
            {
                MuteTimeInSeconds = (int)MuteTime.TotalSeconds,
                Enabled = IsEnabled,
                Blacklist = this.Blacklist.Select(x => x.ToString()),
            };

            var serializedConfig = JsonConvert.SerializeObject(config, Formatting.Indented);

            using (var configStream = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "config", "blacklist.json"), FileMode.Truncate))
            {
                using (var configWriter = new StreamWriter(configStream))
                {
                    await configWriter.WriteAsync(serializedConfig);
                    return true;
                }
            }
        }

        public async Task<bool> LoadConfigurationAsync()
        {
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "config", "blacklist.json"))) return false;

            using (var configStream = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "config", "blacklist.json")))
            {
                using (var configReader = new StreamReader(configStream))
                {
                    var serializedConfig = await configReader.ReadToEndAsync();
                    var config = JsonConvert.DeserializeObject<BlacklistConfig>(serializedConfig);
                    if (config == null) return false;

                    MuteTime = TimeSpan.FromSeconds(config.MuteTimeInSeconds);
                    IsEnabled = config.Enabled;
                    Blacklist = config.Blacklist.Select(x => new Regex(x, RegexOptions.IgnoreCase)).ToList();

                    return true;
                }
            }
        }

        private async Task _client_MessageReceived_Blacklist(SocketMessage m)
        {
            if (m.Author.Id == _client.CurrentUser.Id) return;
            var author = m.Author as IGuildUser;
            if (author == null) return;
            var guild = (m.Channel as IGuildChannel)?.Guild;
            if (guild == null) return;
            if (author.RoleIds.Any(x => _config.BypassIds.Contains(x))) return;
            if (!IsViolatingBlacklist(m.Content)) return;

            var muteRole = guild.GetRole(_config.MuteRoleId);
            await m.DeleteAsync();
            await author.AddRolesAsync(muteRole);

            var dmChannel = await author.GetDMChannelAsync() ?? await author.CreateDMChannelAsync();
            await dmChannel.SendMessageAsync($"You've been muted for {MuteTime} for violating the world blacklist: `{m.Content}`");
            var name = author.Nickname == null
                ? author.Username
                : $"{author.Username} (nickname: {author.Nickname})";
            await _log.LogModMessageAsync($"I automatically muted {name} for {MuteTime} for violating the word blacklist in {(m.Channel as ITextChannel).Mention}: `{m.Content}`");
            var mute = await _records.AddMuteAsync(guild.Id, author.Id, 0, DateTime.UtcNow, DateTime.UtcNow.Add(MuteTime), "N/A (BLACKLIST AUTO-MUTE)");
            _records.DisposeContext();
            _unpunish.Mutes.Add(mute);
        }

        private bool IsViolatingBlacklist(string text) =>
            Blacklist.Any(x => x.IsMatch(text));

        public BlacklistService(DiscordSocketClient c, UnpunishService u, RecordService r, LogService l, CoreConfig config)
        {
            _client = c;
            _unpunish = u;
            _records = r;
            _log = l;
            _config = config;
            Blacklist = new List<Regex>();
            MuteTime = TimeSpan.FromSeconds(3600);
        }
    }
}
