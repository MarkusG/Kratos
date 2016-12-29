using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;
using Kratos.Preconditions;
using Kratos.Services;

namespace Kratos.Modules
{
    [Name("Log Module")]
    [Group("log")]
    [Summary("Manage logging to Discord")]
    public class LogModule : ModuleBase
    {
        private LogService _log;

        [Command("modchannel")]
        [Summary("Set the mod log channel")]
        [RequireCustomPermission("log.manage")]
        public async Task SetModLogChannel([Summary("Channel to which to set the mod log")] ITextChannel channel)
        {
            _log.ModLogChannelId = channel.Id;
            await _log.SaveConfigurationAsync();
            await ReplyAsync(":ok:");
        }

        [Command("serverchannel")]
        [Summary("Set the server log channel")]
        [RequireCustomPermission("log.manage")]
        public async Task SetServerLogChannel([Summary("Channel to which to set the server log")] ITextChannel channel)
        {
            _log.ServerLogChannelId = channel.Id;
            await _log.SaveConfigurationAsync();
            await ReplyAsync(":ok:");
        }

        public LogModule(LogService l)
        {
            _log = l;
        }
    }
}
