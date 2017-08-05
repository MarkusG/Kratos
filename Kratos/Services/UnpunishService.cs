using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using Kratos.Data;
using Kratos.Configs;

namespace Kratos.Services
{
    public class UnpunishService
    {
        private DiscordSocketClient _client;
        private BlacklistService _blacklist;
        private LogService _log;
        private RecordService _records;
        private CoreConfig _config;
        private bool _running;

        public List<Mute> Mutes { get; set; }
        public List<TempBan> Bans { get; set; }

        public async Task GetRecordsAsync()
        {
            Bans = new List<TempBan>(await _records.GetActiveTempBansAsync());
            Mutes = new List<Mute>(await _records.GetActiveMutesAsync());
            _records.DisposeContext();
        }

        public async Task StartAsync()
        {
            _running = true;
            while (_running)
            {
                await Task.Delay(3000);
                var mutesToRemove = new List<Mute>();
                foreach (var m in Mutes.Where(x => DateTime.Compare(DateTime.UtcNow, x.UnmuteAt) > 0 && x.Active))
                {
                    var guild = _client.GetGuild(m.GuildId);
                    var user = guild.GetUser(m.SubjectId);

                    var role = guild.GetRole(_config.MuteRoleId);
                    await user.RemoveRolesAsync(new SocketRole[] { role });
                    var name = user.Nickname == null
                        ? user.Username
                        : $"{user.Username} (nickname: {user.Nickname})";
                    await _log.LogModMessageAsync($":alarm_clock: **{name}#{user.Discriminator} ({m.SubjectId})**'s mute from {m.Timestamp} has expired.");
                    await _records.DeactivateMuteAsync(m.Key);
                    var dmChannel = await user.GetOrCreateDMChannelAsync();
                    await dmChannel.SendMessageAsync($"Your mute in {user.Guild.Name} has expired.");
                    mutesToRemove.Add(m);
                }
                foreach (var m in mutesToRemove)
                    Mutes.Remove(m);

                var bansToRemove = new List<TempBan>();
                foreach (var b in Bans.Where(x => DateTime.Compare(DateTime.UtcNow, x.UnbanAt) > 0 && x.Active))
                {
                    var guild = _client.GetGuild(b.GuildId);
                    await guild.RemoveBanAsync(b.SubjectId);
                    await _log.LogModMessageAsync($":alarm_clock: **{b.SubjectName}#{b.SubjectId} ({b.SubjectId})**'s ban from {b.Timestamp} has expired.");
                    await _records.DeactivateBanAsync(b.Key);
                    bansToRemove.Add(b);
                }
                foreach (var b in bansToRemove)
                    Bans.Remove(b);
            }
        }

        public UnpunishService(DiscordSocketClient c, BlacklistService b, LogService l, RecordService r, CoreConfig config)
        {
            _client = c;
            _blacklist = b;
            _log = l;
            _records = r;
            _config = config;
            Bans = new List<TempBan>();
            Mutes = new List<Mute>();
        }
    }
}
