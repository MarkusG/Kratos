using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Kratos.Configuration;

namespace Kratos.Modules
{
    public class GuildSettingsModule : ModuleBase<SocketCommandContext>
    {
        private GuildConfiguration _guildsConfig;

        [Command("prefix")]
        [Summary("Sets the prefix for the current guild.")]
        public async Task Prefix([Remainder] string prefix)
        {
            var guild = _guildsConfig.GetOrCreate(Context.Guild.Id);
            guild.Prefix = prefix;
            await _guildsConfig.SaveAsync();
            await ReplyAsync($":ok: Prefix for this guild changed to {prefix}");
        }

        public GuildSettingsModule(GuildConfiguration guilds)
        {
            _guildsConfig = guilds;
        }
    }
}
