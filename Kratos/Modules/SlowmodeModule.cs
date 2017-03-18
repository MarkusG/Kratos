using System;
using System.Threading.Tasks;
using System.Text;
using Discord.WebSocket;
using Discord.Commands;
using Kratos.Services;
using Kratos.Preconditions;

namespace Kratos.Modules
{
    [Name("Slowmode Module"), Group("sm")]
    [Summary("Commands for managing slow mode")]
    public class SlowmodeModule : ModuleBase
    {
        private SlowmodeService _service;

        [Command("enable")]
        [Summary("Enables slowmode for the given channel with the given interval")]
        [RequireCustomPermission("slowmode.manage")]
        public async Task Enable([Summary("Channel for which to enable slowmode")] SocketTextChannel channel,
                                 [Summary("Number of seconds between users being able to send messages")] int intervalInSeconds)
        {
            _service.Enable(channel, intervalInSeconds);
            await ReplyAsync(":ok:");
        }

        [Command("disable")]
        [Summary("Disables slowmode for the given channel")]
        [RequireCustomPermission("slowmode.manage")]
        public async Task Disable([Summary("Channel for which to disable slowmode")] SocketTextChannel channel)
        {
            _service.Disable(channel);
            await ReplyAsync(":ok:");
        }

        [Command("mutetime")]
        [Summary("Sets the mute time for slowmode violations")]
        [RequireCustomPermission("slowmode.manage")]
        public async Task MuteTime([Summary("New time to mute users for slowmode violations")] TimeSpan time)
        {
            _service.MuteTime = time;
            await ReplyAsync(":ok:");
        }

        [Command("status")]
        [Summary("Gets current information about the current slowmode settings")]
        [RequireCustomPermission("slowmode.view")]
        public async Task Status()
        {
            var response = new StringBuilder($"**Slowmode settings:**\nMute Time: {_service.MuteTime}\nIntervals:\n");
            foreach (var c in _service.Intervals)
            {
                response.AppendLine($"{c.Key.Mention}: {c.Value} seconds");
            }
            await ReplyAsync(response.ToString());
        }

        public SlowmodeModule(SlowmodeService s)
        {
            _service = s;
        }
    }
}
