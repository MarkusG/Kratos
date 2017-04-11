using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;
using Discord.Commands;
using Humanizer;
using Kratos.Services;
using Kratos.Services.Models;
using Kratos.Preconditions;

namespace Kratos.Modules
{
    [Name("Blacklist Module"), Group("bl")]
    [Summary("Manage the bot's active word filtering (blacklist).")]
    public class BlacklistModule : ModuleBase
    {
        private BlacklistService _service;

        [Command("add"), Alias("+")]
        [Summary("Adds a pattern to a blacklist")]
        [RequireCustomPermission("blacklist.manage")]
        public async Task Add([Summary("The pattern to be added to the blacklist (in quotes)")] string pattern,
                              [Summary("Channel (leave blank to add to global blacklist)")] SocketTextChannel channel = null)
        {
            if (channel == null)
            {
                if (_service.GlobalBlacklist.List == null)
                    _service.GlobalBlacklist.List = new List<Regex>();
                if (!_service.GlobalBlacklist.List.Any(x => x.ToString() == pattern))
                {
                    _service.GlobalBlacklist.List.Add(new Regex(pattern, RegexOptions.IgnoreCase));
                    await ReplyAsync($":ok: `{pattern}` has been added to the blacklist.");
                }
                else
                {
                    await ReplyAsync($":x: `{pattern}` already exists in blacklist.");
                    return;
                }
            }
            else
            {
                if (_service.GlobalBlacklist.List != null && _service.GlobalBlacklist.List.Any(x => x.ToString() == pattern))
                {
                    await ReplyAsync($":warning: `{pattern}` already exists in global blacklist.");
                    return;
                }
                var blacklist = _service.ChannelBlacklists.FirstOrDefault(x => x.Channel.Id == channel.Id);
                if (blacklist == null)
                {
                    var newBlacklist = new ChannelBlacklist(channel, _service.GlobalBlacklist.MuteTime);
                    newBlacklist.List.Add(new Regex(pattern, RegexOptions.IgnoreCase));
                    _service.ChannelBlacklists.Add(newBlacklist);
                }
                else
                {
                    if (blacklist.List.Any(x => x.ToString() == pattern))
                    {
                        await ReplyAsync($":x: `{pattern}` already exists in the blacklist for {channel.Mention}.");
                        return;
                    }
                    blacklist.List.Add(new Regex(pattern, RegexOptions.IgnoreCase));
                }
                await ReplyAsync($":ok: `{pattern}` has been added to the blacklist for {channel.Mention}.");
            }
            await _service.SaveConfigurationAsync();
        }

        [Command("remove"), Alias("-")]
        [Summary("Removes a pattern from a blacklist")]
        [RequireCustomPermission("blacklist.manage")]
        public async Task Remove([Summary("The pattern to be removed from the blacklist (in quotes)")] string pattern,
                                 [Summary("Channel (leave blank to remove from global blacklist")] SocketTextChannel channel = null)
        {
            if (channel == null)
            {
                if (_service.GlobalBlacklist.List == null)
                {
                    await ReplyAsync(":x: No global blacklist found.");
                    return;
                }
                var toRemove = _service.GlobalBlacklist.List.FirstOrDefault(x => x.ToString() == pattern);

                if (toRemove != null)
                {
                    _service.GlobalBlacklist.List.Remove(toRemove);
                    await ReplyAsync($":ok: `{pattern}` has been removed from the blacklist.");
                }
                else
                {
                    await ReplyAsync($":x: `{pattern}` does not exist in blacklist.");
                    return;
                }
            }
            else
            {
                var blacklist = _service.ChannelBlacklists.FirstOrDefault(x => x.Channel.Id == channel.Id);
                if (blacklist == null)
                {
                    await ReplyAsync($":x: No blacklist found for {channel.Mention}.");
                    return;
                }
                var toRemove = blacklist.List.FirstOrDefault(x => x.ToString() == pattern);
                if (toRemove == null)
                {
                    await ReplyAsync($":x: `{pattern}` does not exist in the blacklist for {channel.Mention}.");
                    return;
                }
                blacklist.List.Remove(toRemove);
                if (blacklist.List.Count == 0)
                    _service.ChannelBlacklists.Remove(blacklist);
                await ReplyAsync($":ok: `{pattern}` was removed from the blacklist for {channel.Mention}.");
            }
            await _service.SaveConfigurationAsync();
        }

        [Command("list"), Alias("l")]
        [Summary("Lists all entries in a blacklist")]
        [RequireCustomPermission("blacklist.view")]
        public async Task List([Summary("Channel (leave blank to view global blacklist)")] SocketTextChannel channel = null)
        {
            if (channel == null)
            {
                if (_service.GlobalBlacklist.List == null || _service.GlobalBlacklist.List.Count == 0)
                {
                    await ReplyAsync(":x: No global blacklist found.");
                    return;
                }
                var response = new StringBuilder("**__Global blacklist:__**\n");
                foreach (var s in _service.GlobalBlacklist.List)
                    response.AppendLine($"`{s.ToString()}`");
                await ReplyAsync(response.ToString());
            }
            else
            {
                var blacklist = _service.ChannelBlacklists.FirstOrDefault(x => x.Channel.Id == channel.Id);
                if (blacklist == null)
                {
                    await ReplyAsync($":x: No blacklist found for {channel.Mention}.");
                    return;
                }
                var response = new StringBuilder($"**__Blacklist for {channel.Mention}:__**\n");
                foreach (var s in blacklist.List)
                    response.AppendLine($"`{s.ToString()}`");
                await ReplyAsync(response.ToString());
            }
        }

        [Command("listall"), Alias("la")]
        [Summary("Lists all entries in all blacklists")]
        [RequireCustomPermission("blacklist.view")]
        public async Task ListAll()
        {
            var response = new StringBuilder("**__All blacklists:__**\n");
            if (!(_service.GlobalBlacklist.List == null || _service.GlobalBlacklist.List.Count == 0))
            {
                response.AppendLine("**Global**");
                foreach (var s in _service.GlobalBlacklist.List)
                    response.AppendLine($"`{s.ToString()}`");
            }
            if (_service.ChannelBlacklists.Count != 0)
            {
                foreach (var b in _service.ChannelBlacklists)
                {
                    response.AppendLine($"**{b.Channel.Mention}**");
                    foreach (var p in b.List)
                        response.AppendLine($"`{p.ToString()}`");
                }
            }
            if ((_service.GlobalBlacklist.List == null || _service.GlobalBlacklist.List.Count == 0) && _service.ChannelBlacklists.Count == 0)
            {
                await ReplyAsync(":x: No blacklist data found.");
                return;
            }
            await ReplyAsync(response.ToString());
        }

        [Command("toggle"), Alias("t")]
        [Summary("Enables/disables a blacklist")]
        [RequireCustomPermission("blacklist.manage")]
        public async Task Enable([Summary("Channel (leave blank to toggle global blacklist)")] SocketTextChannel channel = null)
        {
            if (channel == null)
            {
                if (_service.GlobalBlacklist.Enabled)
                {
                    _service.GlobalBlacklist.Enabled = false;
                    await ReplyAsync(":ok: Global blacklist disabled.");
                    await _service.SaveConfigurationAsync();
                    return;
                }
                else
                {
                    _service.GlobalBlacklist.Enabled = true;
                    await ReplyAsync(":ok: Global blacklist enabled.");
                    await _service.SaveConfigurationAsync();
                    return;
                }
            }
            else
            {
                var blacklist = _service.ChannelBlacklists.FirstOrDefault(x => x.Channel.Id == channel.Id);
                if (blacklist == null)
                {
                    await ReplyAsync($":x: No blacklist found for {channel.Mention}");
                    return;
                }
                if (blacklist.Enabled)
                {
                    blacklist.Enabled = false;
                    await ReplyAsync($":ok: Blacklist for {channel.Mention} disabled.");
                    await _service.SaveConfigurationAsync();
                    return;
                }
                else
                {
                    blacklist.Enabled = true;
                    await ReplyAsync($":ok: Blacklist for {channel.Mention} enabled.");
                    await _service.SaveConfigurationAsync();
                    return;
                }
            }
        }

        [Command("mutetime"), Alias("mt")]
        [Summary("Sets the time to mute user for blacklist violations.")]
        [RequireCustomPermission("blacklist.manage")]
        public async Task MuteTime([Summary("hh:mm:ss")] TimeSpan time,
                                   [Summary("Channel (leave blank to set time for global blacklist)")] SocketTextChannel channel = null)
        {
            if (channel == null)
            {
                _service.GlobalBlacklist.MuteTime = time;
                await ReplyAsync($":ok: Global mute time set to {time.Humanize(5)}.");
                await _service.SaveConfigurationAsync();
                return;
            }
            else
            {
                var blacklist = _service.ChannelBlacklists.FirstOrDefault(x => x.Channel.Id == channel.Id);
                if (blacklist == null)
                {
                    await ReplyAsync($":x: No blacklist found for {channel.Mention}.");
                    return;
                }
                else
                {
                    blacklist.MuteTime = time;
                    await ReplyAsync($":ok: Mute time for {channel.Mention} set to {time.Humanize(5)}.");
                    await _service.SaveConfigurationAsync();
                    return;
                }
            }
        }

        [Command("status")]
        [Summary("Gets current information for all blacklists")]
        [RequireCustomPermission("blacklist.view")]
        public async Task GetStatus()
        {
            var response = new StringBuilder("**__Blacklist Status:__**\n");
            response.AppendLine("__Global__");
            response.AppendLine($"Enabled: {_service.GlobalBlacklist.Enabled}");
            response.AppendLine($"Mute time: {_service.GlobalBlacklist.MuteTime.Humanize(5)}");
            foreach (var b in _service.ChannelBlacklists)
            {
                response.AppendLine($"**__{b.Channel.Mention}__**");
                response.AppendLine($"Enabled: {b.Enabled}");
                response.AppendLine($"Mute time: {b.MuteTime.Humanize(5)}");
            }

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
