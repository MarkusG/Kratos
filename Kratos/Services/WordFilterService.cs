using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Discord;
using Discord.WebSocket;
using Humanizer;
using Kratos.Configuration;
using Kratos.Services;
using Kratos.Data;
using Kratos.Results;
using Kratos.Extensions;

namespace Kratos.Services
{
    public class WordFilterService
    {
        private DiscordSocketClient _client;
        private PermissionsService _permissions;
        private GuildConfiguration _guilds;

        public WordFilterConfiguration Config { get; set; } = new WordFilterConfiguration();

        private async Task CheckFilterViolationAsync(SocketMessage m)
        {
            if (m.Author.Id == 72080813948153856) return; // remember to remove this garbage
            if (m.Author.Id == _client.CurrentUser.Id) return;
            var author = m.Author as SocketGuildUser;
            if (author == null) return;

            var result = CheckViolation(m);
            if (!result.Positive) return;

            foreach (var r in author.Roles)
                if (await _permissions.CheckPermissionsAsync(r.Id, "automod.bypass")) return;

            await m.DeleteAsync();
            var config = _guilds.GetOrCreate(author.Guild.Id);
            var muteRole = author.Guild.GetRole(config.MuteRoleId);
            await author.AddRoleAsync(muteRole);
            var timer = new Timer(Unmute, author, result.Filter.MuteTime, TimeSpan.FromMilliseconds(-1));

            var privateMessage = new StringBuilder()
                .AppendLine($"You've been muted for {result.Filter.MuteTime.Humanize(5)} for violating the word filter in {author.Guild.Name}:")
                .AppendLine($"```{m.Content}```");
            await m.Author.SendMessageAsync(privateMessage.ToString()); // TODO support custom messages

            var logMessage = new StringBuilder()
                .AppendLine($"I automatically muted **{author.GetFullName()}** for violating the word filter in {(m.Channel as SocketTextChannel).Mention}:")
                .AppendLine($"```{m.Content}```")
                .AppendLine($"Pattern: `{result.Pattern.ToString()}`");
            var matchValues = result.Matches.ToEnumerable().Select(match => $"`{match.Value}`");
            logMessage.AppendLine($"Matches: {string.Join(", ", matchValues)}");
            var logChannel = _client.GetChannel(config.ModLogId) as SocketTextChannel;
            await logChannel.SendMessageAsync(logMessage.ToString());
        }

        private void Unmute(object o)
        {
            var user = o as SocketGuildUser;
            var config = _guilds.GetOrCreate(user.Guild.Id);
            var muteRole = user.Guild.GetRole(config.MuteRoleId);
            user.RemoveRoleAsync(muteRole);
            var logChannel = _client.GetChannel(config.ModLogId) as SocketTextChannel;
            logChannel.SendMessageAsync($"⏰ **{user.GetFullName()}'s** mute has expired.");
        }

        private WordFilterViolationResult CheckViolation(SocketMessage message)
        {
            var channel = message.Channel as SocketGuildChannel;

            var channelFilter = Config.Filters.FirstOrDefault(f => f.Id == message.Channel.Id && f.Type == WordFilterType.Channel && f.Enabled);
            var guildFilter = Config.Filters.FirstOrDefault(f => f.Id == channel.Guild.Id && f.Type == WordFilterType.Guild && f.Enabled);

            if (channelFilter != null)
            {
                var pattern = channelFilter.Patterns.FirstOrDefault(p => p.IsMatch(message.Content));
                if (pattern != null)
                {
                    var matches = pattern.Matches(message.Content);
                    if (matches.Count > 0)
                        return WordFilterViolationResult.FromPositive(channelFilter, pattern, matches);
                }
            }
            if (guildFilter != null)
            {
                var pattern = guildFilter.Patterns.FirstOrDefault(p => p.IsMatch(message.Content));
                if (pattern != null)
                {
                    var matches = pattern.Matches(message.Content);
                    if (matches.Count > 0)
                        return WordFilterViolationResult.FromPositive(guildFilter, pattern, matches);
                }
            }

            return WordFilterViolationResult.FromNegative();
        }

        public WordFilterService(DiscordSocketClient client, PermissionsService permissions, GuildConfiguration guilds)
        {
            _client = client;
            _client.MessageReceived += CheckFilterViolationAsync;
            _permissions = permissions;
            _guilds = guilds;
        }
    }
}
