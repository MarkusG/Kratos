using System.Threading.Tasks;
using System.Text;
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
        [Summary("Enable slowmode with a given interval")]
        [RequireCustomPermission("slowmode.manage")]
        public async Task Enable([Summary("Number of seconds a user must wait between sending messages")] int interval)
        {
            if (!_service.IsEnabled)
            {
                _service.Enable(interval);
                await ReplyAsync(":ok:");
            }
            else
            {
                await ReplyAsync(":x: Slowmode already enabled");
            }
        }

        [Command("disable")]
        [Summary("Disable slowmode")]
        [RequireCustomPermission("slowmode.manage")]
        public async Task Disable()
        {
            if (_service.IsEnabled)
            {
                _service.Disable();
                await ReplyAsync(":ok:");
            }
            else
            {
                await ReplyAsync(":x: Slowmde not enabled");
            }
        }

        [Command("status")]
        [Summary("Gets current information for slowmode")]
        [RequireCustomPermission("slowmode.view")]
        public async Task GetStatus()
        {
            var response = new StringBuilder("**Slowmode Status:**\n");
            response.AppendLine($"Enabled: {_service.IsEnabled}");
            response.AppendLine($"Interval in seconds: {_service.IntervalInSeconds}");
            await ReplyAsync(response.ToString());
        }

        public SlowmodeModule(SlowmodeService s)
        {
            _service = s;
        }
    }
}
