using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Discord.WebSocket;
using Humanizer;
using Kratos.Configs;
using Kratos.Data;
using Kratos.Services.Models;
using Kratos.Services.Results;

namespace Kratos.Services
{
    public class BlacklistService
    {
        private DiscordSocketClient _client;
        private UnpunishService _unpunish;
        private LogService _log;
        private RecordService _records;
        private CoreConfig _config;

        public List<ChannelBlacklist> ChannelBlacklists { get; private set; }

        public GlobalBlacklist GlobalBlacklist { get; private set; }

        public async Task<bool> SaveConfigurationAsync()
        {
            var config = BlacklistConfig.FromService(this);
            var serializedConfig = JsonConvert.SerializeObject(config, Formatting.Indented);

            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "config", "blacklist.json")))
                File.Create(Path.Combine(Directory.GetCurrentDirectory(), "config", "blacklist.json")).Dispose();
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

                    foreach (var b in config.ChannelBlacklists)
                    {
                        ChannelBlacklists.Add(new ChannelBlacklist
                        {
                            Channel = _client.GetChannel(b.ChannelId) as SocketTextChannel,
                            List = b.List.Select(x => new Regex(x)).ToList(),
                            MuteTime = b.MuteTime,
                            Enabled = b.Enabled
                        });
                    }

                    GlobalBlacklist.List = config.GlobalList.Select(x => new Regex(x)).ToList();
                    GlobalBlacklist.MuteTime = config.GlobalMuteTime;
                    GlobalBlacklist.Enabled = config.GlobalEnabled;

                    return true;
                }
            }
        }

        private async Task _client_MessageReceived_Blacklist(SocketMessage m)
        {
            if (m.Author.Id == _client.CurrentUser.Id) return;
            var author = m.Author as SocketGuildUser;
            if (author == null) return;
            var guild = (m.Channel as SocketGuildChannel)?.Guild;
            if (guild == null) return;
            if (author.Roles.Select(r => r.Id).Any(x => _config.BypassIds.Contains(x))) return;
            var violation = CheckViolation(m);
            if (!violation.IsViolating) return;

            var muteRole = guild.GetRole(_config.MuteRoleId);
            await m.DeleteAsync();
            await author.AddRoleAsync(muteRole);

            var dmChannel = await author.GetOrCreateDMChannelAsync();
            Mute mute = null;
            if (violation.Blacklist == null)
            {
                await dmChannel.SendMessageAsync($"You've been muted for {GlobalBlacklist.MuteTime.Humanize(5)} for violating the world blacklist: `{m.Content}`");
                var name = author.Nickname == null
                    ? author.Username
                    : $"{author.Username} (nickname: {author.Nickname})";
                await _log.LogModMessageAsync($"I automatically muted **{name} ({author.Id})** for {GlobalBlacklist.MuteTime.Humanize(5)} for violating the word blacklist in {(m.Channel as SocketTextChannel).Mention}: `{m.Content}`");
                mute = await _records.AddMuteAsync(new Mute
                {
                    GuildId = guild.Id,
                    SubjectId = author.Id,
                    ModeratorId = 0,
                    Timestamp = DateTime.UtcNow,
                    UnmuteAt = DateTime.UtcNow.Add(GlobalBlacklist.MuteTime),
                    Reason = "N/A (BLACKLIST AUTO-MUTE)",
                    Active = true
                });
            }
            else
            {
                await dmChannel.SendMessageAsync($"You've been muted for {violation.Blacklist.MuteTime.Humanize(5)} for violating the world blacklist: `{m.Content}`");
                var name = author.Nickname == null
                    ? author.Username
                    : $"{author.Username} (nickname: {author.Nickname})";
                await _log.LogModMessageAsync($"I automatically muted **{name} ({author.Id})** for {violation.Blacklist.MuteTime.Humanize(5)} for violating the word blacklist in {(m.Channel as SocketTextChannel).Mention}: `{m.Content}`");
                mute = await _records.AddMuteAsync(new Mute
                {
                    GuildId = guild.Id,
                    SubjectId = author.Id,
                    ModeratorId = 0,
                    Timestamp = DateTime.UtcNow,
                    UnmuteAt = DateTime.UtcNow.Add(violation.Blacklist.MuteTime),
                    Reason = "N/A (BLACKLIST AUTO-MUTE)",
                    Active = true
                });
            }
            _records.DisposeContext();
            _unpunish.Mutes.Add(mute);
        }

        private BlacklistViolationResult CheckViolation(SocketMessage m)
        {
            if (GlobalBlacklist.CheckViolation(m.Content)) return new BlacklistViolationResult { IsViolating = true };
            var blacklist = ChannelBlacklists.FirstOrDefault(x => x.Channel == m.Channel as SocketTextChannel);
            if (blacklist == null) return new BlacklistViolationResult { IsViolating = false };
            return new BlacklistViolationResult { Blacklist = blacklist, IsViolating = blacklist.CheckViolation(m.Content) };
        }

        public BlacklistService(DiscordSocketClient c, UnpunishService u, RecordService r, LogService l, CoreConfig config)
        {
            _client = c;
            _unpunish = u;
            _records = r;
            _log = l;
            _config = config;
            ChannelBlacklists = new List<ChannelBlacklist>();
            GlobalBlacklist = new GlobalBlacklist();
            _client.MessageReceived += _client_MessageReceived_Blacklist;
        }
    }
}
