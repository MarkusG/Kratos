using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Discord;
using Discord.WebSocket;
using Kratos.Configs;

namespace Kratos.Services
{
    public class RatelimitService
    {
        private DiscordSocketClient _client;
        private CoreConfig _config;
        private RecordService _records;
        private UnpunishService _unpunish;
        private LogService _log;
        private Dictionary<ulong, DateTime[]> _lastthreemessages;

        public int Limit { get; set; }
        public int MuteTime { get; set; }
        public bool IsEnabled { get; private set; }

        public void Enable(int limit)
        {
            Limit = limit;
            _client.MessageReceived += _client_MessageReceived_Ratelimit;
            IsEnabled = true;
        }

        public void Disable()
        {
            _client.MessageReceived -= _client_MessageReceived_Ratelimit;
            IsEnabled = false;
        }

        public async Task<bool> SaveConfigurationAsync()
        {
            var config = new RatelimitConfig
            {
                Limit = Limit,
                MuteTime = MuteTime,
                IsEnabled = IsEnabled
            };

            var serializedConfig = JsonConvert.SerializeObject(config);

            using (var configStream = File.OpenWrite(@"config\ratelimit.json"))
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
            if (!File.Exists(@"config\ratelimit.json")) return false;

            using (var configStream = File.OpenRead(@"config\ratelimit.json"))
            {
                using (var configReader = new StreamReader(configStream))
                {
                    var serializedConfig = await configReader.ReadToEndAsync();
                    var config = JsonConvert.DeserializeObject<RatelimitConfig>(serializedConfig);
                    if (config == null) return false;

                    Limit = config.Limit;
                    MuteTime = config.MuteTime;
                    IsEnabled = config.IsEnabled;

                    return true;
                }
            }
        }

        private async Task _client_MessageReceived_Ratelimit(SocketMessage m)
        {
            if (m.Author.Id == _client.CurrentUser.Id) return;
            var message = m as SocketUserMessage;
            if (m == null) return;
            var author = m.Author as IGuildUser;
            if (author == null) return;

            if (author.RoleIds.Any(x => _config.BypassIds.Contains(x))) return;

            if (!_lastthreemessages.ContainsKey(author.Id))
                _lastthreemessages.Add(author.Id, new DateTime[3]);

            // if it has been at least <limit> since the user's last 3 messages were sent
            if (_lastthreemessages[author.Id].All(x => DateTime.Compare(DateTime.UtcNow, x.AddSeconds(Limit)) > 0))
            {
                _lastthreemessages[author.Id][0] = _lastthreemessages[author.Id][1];
                _lastthreemessages[author.Id][1] = _lastthreemessages[author.Id][2];
                _lastthreemessages[author.Id][2] = DateTime.UtcNow;
            }
            else
            {
                var muteRole = author.Guild.GetRole(_config.MuteRoleId);
                await author.AddRolesAsync(muteRole);

                var dmChannel = await author.GetDMChannelAsync() ?? await author.CreateDMChannelAsync();
                await dmChannel.SendMessageAsync($"You've been muted for {TimeSpan.FromSeconds(MuteTime)} for ratelimiting: `{m.Content}`");
                var name = author.Nickname == null
                    ? author.Username
                    : $"{author.Username} (nickname: {author.Nickname})";
                await _log.LogModMessage($"I automatically muted {name} for ratelimiting in {(m.Channel as ITextChannel).Mention}: `{m.Content}`");
                var timestamp = (ulong)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                var unmuteAt = (ulong)DateTime.UtcNow.Add(TimeSpan.FromSeconds(MuteTime)).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                var mute = await _records.AddMuteAsync(author.Guild.Id, author.Id, 0, timestamp, unmuteAt, "N/A (RATELIMIT AUTO-MUTE)");
                _records.DisposeContext();
                _unpunish.Mutes.Add(mute);
            }
        }

        public RatelimitService(DiscordSocketClient c, CoreConfig config, RecordService r, UnpunishService u, LogService l)
        {
            _client = c;
            _config = config;
            _records = r;
            _unpunish = u;
            _log = l;
            _lastthreemessages = new Dictionary<ulong, DateTime[]>();
        }
    }
}
