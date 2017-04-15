using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using Kratos.Configs;
using Kratos.Data;
using Humanizer;

namespace Kratos.Services
{
    public class SlowmodeService // TODO make this channel specific
    {
        private DiscordSocketClient _client;
        private LogService _log;
        private UnpunishService _unpunish;
        private RecordService _records;
        private CoreConfig _config;
        private Dictionary<ulong, Dictionary<SocketTextChannel, DateTime>> _lastMessage; // Represents the last time a user sent a message for a channel

        public Dictionary<SocketTextChannel, int> Intervals { get; private set; }
        public TimeSpan MuteTime { get; set; } = TimeSpan.FromMinutes(1);

        public void Enable(SocketTextChannel channel, int interval)
        {
            if (Intervals.ContainsKey(channel))
                Intervals[channel] = interval;
            else
                Intervals.Add(channel, interval);
        }

        public void Disable(SocketTextChannel channel)
        {
            if (Intervals.ContainsKey(channel))
                Intervals.Remove(channel);
        }

        private async Task _client_MessageReceived_Slowmode(SocketMessage m)
        {
            if (m.Author.Id == _client.CurrentUser.Id) return; // Ignore messages from the bot itself
            if (!(m.Author is SocketGuildUser author)) return; // Ignore messages that do not come from a guild
            if (author.Roles.Select(r => r.Id).Any(x => _config.BypassIds.Contains(x))) return; // Ignore messages from privileged users

            var channel = m.Channel as SocketTextChannel;

            if (!Intervals.ContainsKey(channel)) return; // Ignore channels for which slowmode is not enabled

            var interval = Intervals[channel];

            if (!_lastMessage.ContainsKey(author.Id))
            {
                var dictionary = new Dictionary<SocketTextChannel, DateTime>();
                dictionary.Add(channel, DateTime.UtcNow);
                _lastMessage.Add(author.Id, dictionary);
            }
            else
            {
                if (DateTime.UtcNow.Subtract(_lastMessage[author.Id][channel]).TotalSeconds >= Intervals[channel]) // If the user's message was sent after the interval was up
                {
                    _lastMessage[author.Id][channel] = DateTime.UtcNow;
                }
                else
                {
                    // Delete message and mute user
                    await m.DeleteAsync();
                    var muteRole = author.Guild.GetRole(_config.MuteRoleId);
                    await author.AddRoleAsync(muteRole);
                    // author.Guild.Id, author.Id, 0, DateTime.UtcNow, DateTime.UtcNow.Add(MuteTime), "N/A (SLOWMODE AUTO-MUTE)"
                    var mute = await _records.AddMuteAsync(new Mute
                    {
                        GuildId = author.Guild.Id,
                        SubjectId = author.Id,
                        ModeratorId = 0,
                        Timestamp = DateTime.UtcNow,
                        UnmuteAt = DateTime.UtcNow.Add(MuteTime),
                        Reason = "N/A (SLOWMODE AUTO-MUTE)",
                        Active = true

                    });
                    _unpunish.Mutes.Add(mute);
                    _records.DisposeContext();
                    await _log.LogModMessageAsync($"Automatically muted {author.Nickname ?? author.Username}#{author.Discriminator} ({author.Id})'s message in {channel.Mention} for {MuteTime.Humanize(5)} for violating slowmode: `{m.Content}`");
                }
            }
        }

        public SlowmodeService(DiscordSocketClient c, LogService l, UnpunishService u, RecordService r, CoreConfig config)
        {
            _client = c;
            _client.MessageReceived += _client_MessageReceived_Slowmode;
            _log = l;
            _unpunish = u;
            _records = r;
            _config = config;
            _lastMessage = new Dictionary<ulong, Dictionary<SocketTextChannel, DateTime>>();
            Intervals = new Dictionary<SocketTextChannel, int>();
        }
    }
}
