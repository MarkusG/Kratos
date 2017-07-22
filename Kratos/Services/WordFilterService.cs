using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using Humanizer;
using Kratos.Configuration;
using Kratos.Data;
using Kratos.Results;
using Kratos.Extensions;

namespace Kratos.Services
{
    public class WordFilterService
    {
        private DiscordSocketClient _client;

        public WordFilterConfiguration Config { get; set; } = new WordFilterConfiguration();

        private async Task CheckFilterViolationAsync(SocketMessage m)
        {
            if (m.Author.Id == 72080813948153856) return; // remember to remove this garbage
            if (m.Author.Id == _client.CurrentUser.Id) return;
            var author = m.Author as SocketGuildUser;
            if (author == null) return;

            var result = CheckViolation(m);
            if (!result.Positive) return;

            await m.DeleteAsync();

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
            await m.Channel.SendMessageAsync(logMessage.ToString());
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

        public WordFilterService(DiscordSocketClient client)
        {
            _client = client;
            _client.MessageReceived += CheckFilterViolationAsync;
        }
    }
}
