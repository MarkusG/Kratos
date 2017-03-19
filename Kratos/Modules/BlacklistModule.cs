using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using Discord.Commands;
using Kratos.Services;
using Kratos.Preconditions;

namespace Kratos.Modules
{
    [Name("Blacklist Module"), Group("bl")]
    [Summary("Manage the bot's active word filtering (blacklist).")]
    public class BlacklistModule : ModuleBase
    {
        private BlacklistService _service;

        [Command("add"), Alias("+")]
        [Summary("Adds a pattern to the blacklist")]
        [RequireCustomPermission("blacklist.manage")]
        public async Task Add([Summary("The pattern to be added to the blacklist"), Remainder] string pattern)
        {
            if (!_service.Blacklist.Any(x => x.ToString() == pattern))
            {
                _service.Blacklist.Add(new Regex(pattern, RegexOptions.IgnoreCase));
                await ReplyAsync($":ok: `{pattern}` has been added to the blacklist.");
                await _service.SaveConfigurationAsync();
            }
            else
                await ReplyAsync($":x: `{pattern}` already exists in blacklist.");
        }

        [Command("remove"), Alias("-")]
        [Summary("Removes a pattern from the blacklist")]
        [RequireCustomPermission("blacklist.manage")]
        public async Task Remove([Summary("The pattern to be removed from the blacklist"), Remainder] string pattern)
        {
            var toRemove = _service.Blacklist.FirstOrDefault(x => x.ToString() == pattern);

            if (toRemove != null)
            {
                _service.Blacklist.Remove(toRemove);
                await _service.SaveConfigurationAsync();
                await ReplyAsync($":ok: `{pattern}` has been removed from the blacklist.");
            }
            else
            {
                await ReplyAsync($":x: `{pattern}` does not exist in blacklist.");
            }
        }

        [Command("list"), Alias("l")]
        [Summary("Lists all entries in the blacklist")]
        [RequireCustomPermission("blacklist.view")]
        public async Task List()
        {
            StringBuilder listResponse = new StringBuilder("**BLACKLIST:**\n");
            foreach (var s in _service.Blacklist)
                listResponse.AppendLine(s.ToString());
            await ReplyAsync(listResponse.ToString());
        }

        [Command("enable"), Alias("e")]
        [Summary("Enables the blacklist")]
        [RequireCustomPermission("blacklist.manage")]
        public async Task Enable()
        {
            if (!_service.IsEnabled)
            {
                _service.Enable();
                await _service.SaveConfigurationAsync();
                await ReplyAsync(":ok:");
            }
            else
            {
                await ReplyAsync(":x: Blacklist already enabled");
            }
        }
        
        [Command("disable"), Alias("d")]
        [Summary("Disables the blacklist")]
        [RequireCustomPermission("blacklist.manage")]
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
                await ReplyAsync(":x: Blacklist not enabled");
            }
        }

        [Command("mutetime"), Alias("mt")]
        [Summary("Sets the time to mute user for blacklist violations.")]
        [RequireCustomPermission("blacklist.manage")]
        public async Task MuteTime([Summary("hh:mm:ss")] TimeSpan time)
        {
            _service.MuteTime = time;
            await _service.SaveConfigurationAsync();
            await ReplyAsync(":ok:");
        }

        [Command("status")]
        [Summary("Gets current information for the blacklist")]
        [RequireCustomPermission("blacklist.view")]
        public async Task GetStatus()
        {
            var response = new StringBuilder("**Blacklist Status:**\n");
            response.AppendLine($"Enabled: {_service.IsEnabled}");
            response.AppendLine($"Mute time: {_service.MuteTime}");
            await ReplyAsync(response.ToString());
        }

        [Command("saveconfig"), Alias("sc")]
        [Summary("Save the current configuration of the blacklist, including the list itself")]
        [RequireCustomPermission("blacklist.admin")]
        public async Task SaveConfig()
        {
            var success = await _service.SaveConfigurationAsync();
            await ReplyAsync(success ? ":ok:" : ":x: Failed to save config.");
        }

        [Command("reloadconfig"), Alias("rc")]
        [Summary("Reloads the configuration of the blacklist, including the list itself, from config\\blacklist.json")]
        [RequireCustomPermission("blacklist.admin")]
        public async Task ReloadConfig()
        {
            var success = await _service.LoadConfigurationAsync();
            await ReplyAsync(success ? ":ok:" : ":x: Failed to load config. Please configure the blacklist and save the config.");
        }

        public BlacklistModule(BlacklistService bm)
        {
            _service = bm;
        }
    }
}
