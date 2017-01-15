using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Kratos.Services;
using Kratos.Configs;
using Kratos.Preconditions;

namespace Kratos.Modules
{
    [Name("Blacklist Module"), Group("bl")]
    [Summary("Manage the bot's active word filtering (blacklist).")]
    public class BlacklistModule : ModuleBase
    {
        private BlacklistService _service;

        [Command("add"), Alias("+")]
        [Summary("Adds a phrase to the blacklist")]
        [RequireCustomPermission("blacklist.manage")]
        public async Task Add([Summary("The phrase to be added to the blacklist"), Remainder] string phrase)
        {
            if (!_service.Blacklist.Contains(phrase))
            {
                _service.Blacklist.Add(phrase);
                await ReplyAsync($":ok: `{phrase}` has been added to the blacklist.");
                await _service.SaveConfigurationAsync();
            }
            else
                await ReplyAsync($":x: `{phrase}` already exists in blacklist.");
        }

        [Command("remove"), Alias("-")]
        [Summary("Removes a phrase from the blacklist")]
        [RequireCustomPermission("blacklist.manage")]
        public async Task Remove([Summary("The phrase to be removed from the blacklist"), Remainder] string phrase)
        {
            if (_service.Blacklist.Contains(phrase))
            {
                _service.Blacklist.Remove(phrase);
                await _service.SaveConfigurationAsync();
                await ReplyAsync($":ok: `{phrase}` has been removed from the blacklist.");
            }
            else
            {
                await ReplyAsync($":x: `{phrase}` does not exist in blacklist.");
            }
        }

        [Command("list"), Alias("l")]
        [Summary("Lists all entries in the blacklist")]
        [RequireCustomPermission("blacklist.view")]
        public async Task List()
        {
            StringBuilder listResponse = new StringBuilder("**WORD BLACKLIST:**\n");
            foreach (var s in _service.Blacklist)
                listResponse.AppendLine(s);
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
            _service.MuteTime = (int)time.TotalSeconds;
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
