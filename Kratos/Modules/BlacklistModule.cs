using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.InteractiveCommands;
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
        private CoreConfig _config;

        [Command("add"), Alias("+")]
        [Summary("Adds a phrase to the blacklist")]
        [RequireCustomPermission("blacklist.manage")]
        public async Task Add([Summary("The phrase to be added to the blacklist"), Remainder] string phrase)
        {
            if (!_service.Blacklist.Contains(phrase))
            {
                _service.Blacklist.Add(phrase);
                await ReplyAsync($":ok: `{phrase}` has been added to the blacklist.");
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
                await ReplyAsync(":ok:");
            }
            else
                await ReplyAsync(":x: Blacklist already enabled");
        }
        
        [Command("disable"), Alias("d")]
        [Summary("Disables the blacklist")]
        [RequireCustomPermission("blacklist.manage")]
        public async Task Disable()
        {
            if (_service.IsEnabled)
            {
                _service.Disable();
                await ReplyAsync(":ok:");
            }
            else
                await ReplyAsync(":x: Blacklist not enabled");
        }

        [Command("setmuterole"), Alias("smr")]
        [Summary("Sets the role to assign to users when they say a blacklisted word")]
        [RequireCustomPermission("blacklist.manage")]
        public async Task SetMuteRole([Summary("The role to give users upon violations of the word blacklist")] IRole role)
        {
            _service.MuteRoleId = role.Id;
            await ReplyAsync($":ok: Mute role set to {role.Mention}");
        }

        [Command("addbypassrole"), Alias("abp")]
        [Summary("Adds a role for which the bot will ignore blacklist violations")]
        [RequireCustomPermission("blacklist.manage")]
        public async Task AddBypassRole([Summary("The role to add to the ignore list")] IRole role)
        {
            if (!_service.BypassIds.Contains(role.Id))
            {
                _service.BypassIds.Add(role.Id);
                await ReplyAsync($":ok: Will now ignore blacklist violations from {role.Mention}");
            }
            else
                await ReplyAsync(":x: That role is already in the bypass list.");
        }

        [Command("removebypassrole"), Alias("rbp")]
        [Summary("Removes a role for which the bot will ignore blacklist violations")]
        [RequireCustomPermission("blacklist.manage")]
        public async Task RemoveBypassRole([Summary("The role to remove from the ignore list")] IRole role)
        {
            if (_service.BypassIds.Contains(role.Id))
            {
                _service.BypassIds.Remove(role.Id);
                await ReplyAsync($":ok: Will no longer ignore blacklist violations from {role.Mention}");
            }
            else
                await ReplyAsync(":x: That role is not on the bypass list.");
        }

        [Command("listbypassroles"), Alias("lbp")]
        [Summary("Lists all the roles for which the bot will ignore blacklist violations")]
        [RequireCustomPermission("blacklist.view")]
        public async Task ListBypassRoles()
        {
            StringBuilder response = new StringBuilder("**BYPASS ROLES:**\n");
            foreach (ulong i in _service.BypassIds)
                response.AppendLine(Context.Guild.GetRole(i).Name);
            await ReplyAsync(response.ToString());
        }

        [Command("setlogchannel"), Alias("slc")]
        [Summary("Sets the bot's log channel")]
        [RequireCustomPermission("blacklist.manage")]
        public async Task SetLogChannel([Summary("The channel in which the bot will log its actions")] ITextChannel channel)
        {
            _service.LogChannelId = channel.Id;
            await ReplyAsync($":ok: Log channel set to #{channel.Name}");
        }

        [Command("mutetime"), Alias("mt")]
        [Summary("Sets the time to mute user for blacklist violations.")]
        [RequireCustomPermission("blacklist.manage")]
        public async Task MuteTime(TimeSpan time)
        {
            _service.MuteTime = (int)time.TotalMilliseconds;
            await ReplyAsync(":ok:");
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

        public BlacklistModule(BlacklistService bm, CoreConfig c)
        {
            _config = c;
            _service = bm;
        }
    }
}
