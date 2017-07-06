using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using Discord.WebSocket;
using Discord.Commands;
using Kratos.Preconditions;
using Kratos.Services;
using Kratos.Data;
using Kratos.Results;

namespace Kratos.Modules
{
    [Name("Word Filter Module"), Group("wf")]
    [Summary("Provides commands for managing the word filter.")]
    public class WordFilterModule : ModuleBase<SocketCommandContext>
    {
        public WordFilterService _service;

        [Command("add"), Alias("+")]
        [Summary("Adds a pattern to the word filter")]
        [Permission("filter.manage")]
        public async Task<RuntimeResult> AddAsync([Summary("The pattern to be added to the filter (in quotes)")] string pattern,
                                                  [Summary("Channel (leave blank to add to the guild's filter")] SocketTextChannel channel = null)
        {
            WordFilter filter;

            if (channel == null)
                filter = _service.Config.Filters.FirstOrDefault(f => f.Id == Context.Guild.Id);
            else
                filter = _service.Config.Filters.FirstOrDefault(f => f.Id == channel.Id);

            if (filter == null)
            {
                filter = new WordFilter(Context.Guild.Id, channel == null ? WordFilterType.Guild : WordFilterType.Channel);
                filter.Patterns.Add(new Regex(pattern));
                _service.Config.Filters.Add(filter);
            }
            else
            {
                if (filter.Patterns.Any(p => p.ToString() == pattern))
                    return SimpleRuntimeResult.FromWarning("Pattern already present in filter.");
                filter.Patterns.Add(new Regex(pattern));
            }
            await ReplyAsync("🆗");
            return SimpleRuntimeResult.FromSuccess();
        }

        [Command("remove"), Alias("-")]
        [Summary("Removes a pattern from the word filter")]
        [Permission("filter.manage")]
        public async Task<RuntimeResult> RemoveAsync([Summary("The pattern to remove from the filter (in quotes)")] string pattern,
                                                     [Summary("Channel (leave blank to remove from the guild's filter")] SocketTextChannel channel = null)
        {
            WordFilter filter;

            if (channel == null)
                filter = _service.Config.Filters.FirstOrDefault(f => f.Id == Context.Guild.Id);
            else
                filter = _service.Config.Filters.FirstOrDefault(f => f.Id == channel.Id);

            if (filter == null)
                return SimpleRuntimeResult.FromWarning("No filter found.");
            else
            {
                var entry = filter.Patterns.FirstOrDefault(p => p.ToString() == pattern);
                if (entry == null)
                    return SimpleRuntimeResult.FromWarning("Pattern not found.");
                filter.Patterns.Remove(entry);
                await ReplyAsync("🆗");
                return SimpleRuntimeResult.FromSuccess();
            }
        }

        [Command("clear")]
        [Summary("Clears the filter")]
        [Permission("filter.manage")]
        public async Task<RuntimeResult> ClearAsync([Summary("Channel (leave blank to view the guild's blacklist)")] SocketTextChannel channel = null)
        {
            WordFilter filter;

            if (channel == null)
                filter = _service.Config.Filters.FirstOrDefault(f => f.Id == Context.Guild.Id);
            else
                filter = _service.Config.Filters.FirstOrDefault(f => f.Id == channel.Id);

            if (filter == null)
                return SimpleRuntimeResult.FromWarning("No filter found.");

            _service.Config.Filters.Remove(filter);
            await ReplyAsync("🆗");
            return SimpleRuntimeResult.FromSuccess();
        }

        [Command("list")]
        [Summary("Lists all patterns in the word filter")]
        [Permission("filter.view")]
        public async Task<RuntimeResult> ListAsync([Summary("Channel (leave blank to view the guild's blacklist)")] SocketTextChannel channel = null)
        {
            WordFilter filter;

            if (channel == null)
                filter = _service.Config.Filters.FirstOrDefault(f => f.Id == Context.Guild.Id);
            else
                filter = _service.Config.Filters.FirstOrDefault(f => f.Id == channel.Id);

            if (filter == null)
                return SimpleRuntimeResult.FromFailure("No filter found.");

            var response = new StringBuilder(channel == null ? "**Word filter for this guild:**" : $"**Word filter for {channel.Mention}:**\n");
            foreach (var p in filter.Patterns)
                response.AppendLine($"`{p.ToString()}`");
            await ReplyAsync(response.ToString());
            return SimpleRuntimeResult.FromSuccess();
        }

        public WordFilterModule(WordFilterService wf)
        {
            _service = wf;
        }
    }
}
