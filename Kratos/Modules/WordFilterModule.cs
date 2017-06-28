using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using Discord.WebSocket;
using Discord.Commands;
using Kratos.Preconditions;
using Kratos.Services;
using Kratos.Data;

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
        public async Task AddAsync([Summary("The pattern to be added to the filter (in quotes)")] string pattern,
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
                {
                    await ReplyAsync(":x: Pattern already present in filter.");
                    return;
                }
                filter.Patterns.Add(new Regex(pattern));
            }
            await ReplyAsync(":ok:");
        }

        public WordFilterModule(WordFilterService wf)
        {
            _service = wf;
        }
    }
}
