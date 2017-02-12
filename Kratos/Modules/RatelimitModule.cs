using System.Threading.Tasks;
using System.Text;
using Discord;
using Discord.Commands;
using Kratos.Preconditions;
using Kratos.Services;

namespace Kratos.Modules
{
    [Name("Ratelimit Module"), Group("rl")]
    [Summary("Commands to manage ratelimits.")]
    public class RatelimitModule : ModuleBase
    {
        private RatelimitService _service;

        [Command("enable")]
        [Summary("Enables ratelimiting with a given second limit")]
        [RequireCustomPermission("ratelimit.manage")]
        public async Task Enable([Summary("The maximum amount of time (in seconds) in which a user can send 3 messages")] int limit)
        {
            if (!_service.IsEnabled)
            {
                _service.Enable(limit);
                await _service.SaveConfigurationAsync();
                await ReplyAsync(":ok:");
            }
            else
            {
                await ReplyAsync(":x: Ratelimit already enabled.");
            }
        }

        [Command("disable")]
        [Summary("Disables ratelimiting")]
        [RequireCustomPermission("ratelimit.manage")]
        public async Task Disable()
        {
            if (_service.IsEnabled)
            {
                _service.Disable();
                await _service.SaveConfigurationAsync();
                await ReplyAsync(":ok:");
            }
            else
            {
                await ReplyAsync(":x: Ratelimit not enabled");
            }
        }

        [Command("setlimit")]
        [Summary("Sets the maximum amount of time (in seconds) in which a user can send 3 messages")]
        [RequireCustomPermission("ratelimit.manage")]
        public async Task SetLimit([Summary("New limit")] int limit)
        {
            _service.Limit = limit;
            await ReplyAsync(":ok:");
        }

        [Command("setmutetime"), Alias("mutetime")]
        [Summary("Sets the time to mute for users who get ratelimited")]
        [RequireCustomPermission("ratelimit.manage")]
        public async Task SetMuteTime([Summary("Mute time")] int timeInSeconds)
        {
            _service.MuteTime = timeInSeconds;
            await ReplyAsync(":ok:");
        }

        [Command("ignore")]
        [Summary("Toggle ignoring a channel for ratelimiting.")]
        [RequireCustomPermission("ratelimit.manage")]
        public async Task IgnoreChannel([Summary("Channel for which to toggle ignoring")] IGuildChannel channel)
        {
            if (_service.IgnoredChannels.Contains(channel.Id))
            {
                _service.IgnoredChannels.Remove(channel.Id);
                await _service.SaveConfigurationAsync();
                await ReplyAsync($":ok: Now ignoring ratelimit in {(channel as ITextChannel).Mention}.");
                return;
            }
            else
            {
                _service.IgnoredChannels.Add(channel.Id);
                await _service.SaveConfigurationAsync();
                await ReplyAsync($":ok: No longer ignoring ratelimit in {(channel as ITextChannel).Mention}.");
                return;
            }
        }

        [Command("status")]
        [Summary("Gets current information for ratelimiting")]
        [RequireCustomPermission("ratelimit.view")]
        public async Task GetStatus()
        {
            var response = new StringBuilder($"**Ratelimit Status:**\n");
            response.AppendLine($"Enabled: {_service.IsEnabled}");
            response.AppendLine($"Limit in seconds: {_service.Limit}");
            response.AppendLine($"Mute time: {_service.MuteTime}");
            await ReplyAsync(response.ToString());
        }

        public RatelimitModule(RatelimitService r)
        {
            _service = r;
        }
    }
}
