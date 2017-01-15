using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Kratos.Services;
using Kratos.Configs;
using Kratos.Preconditions;

namespace Kratos.Modules
{
    [Name("Core Module"), Group("core")]
    [Summary("The bot's core commands.")]
    public class CoreModule : ModuleBase
    {
        private DiscordSocketClient _client;
        private CommandService _commands;
        private CoreConfig _config;
        private BlacklistService _blacklist;

        [Command("help"), Summary("Displays this help message")]
        public async Task Help() =>
            await ReplyAsync("https://github.com/MarkusGordathian/Kratos/wiki/Commands");

        [Command("ping")]
        [Summary("Returns \"Pong!\"")]
        public async Task Ping()
        {
            await ReplyAsync($"Pong! My latency is currently {_client.Latency}ms.");
        }

        [Command("setmuterole"), Alias("smr")]
        [Summary("Sets the mute role")]
        [RequireCustomPermission("core.manage")]
        public async Task SetMuteRole(IRole role)
        {
            _config.MuteRoleId = role.Id;
            await _config.SaveAsync();
            await ReplyAsync(":ok:");
        }

        [Command("addbypassrole"), Alias("abp")]
        [Summary("Adds a role for which the bot will ignore active protection")]
        [RequireCustomPermission("blacklist.manage")]
        public async Task AddBypassRole([Summary("The role to add to the ignore list")] IRole role)
        {
            if (!_config.BypassIds.Contains(role.Id))
            {
                _config.BypassIds.Add(role.Id);
                await _config.SaveAsync();
                await ReplyAsync($":ok: Will now ignore active protection for {role.Mention}");
            }
            else
                await ReplyAsync(":x: That role is already in the bypass list.");
        }

        [Command("removebypassrole"), Alias("rbp")]
        [Summary("Removes a role for which the bot will ignore active protection")]
        [RequireCustomPermission("blacklist.manage")]
        public async Task RemoveBypassRole([Summary("The role to remove from the ignore list")] IRole role)
        {
            if (_config.BypassIds.Contains(role.Id))
            {
                _config.BypassIds.Remove(role.Id);
                await _config.SaveAsync();
                await ReplyAsync($":ok: Will no longer ignore active protection for {role.Mention}");
            }
            else
                await ReplyAsync(":x: That role is not on the bypass list.");
        }

        [Command("listbypassroles"), Alias("lbp")]
        [Summary("Lists all the roles for which the bot will ignore active protection")]
        [RequireCustomPermission("blacklist.view")]
        public async Task ListBypassRoles()
        {
            StringBuilder response = new StringBuilder("**BYPASS ROLES:**\n");
            foreach (var i in _config.BypassIds)
                response.AppendLine(Context.Guild.GetRole(i).Name);
            await ReplyAsync(response.ToString());
        }

        //[Command("edituser"), Summary("Edits the bot's account")]
        //public async Task Edit([Summary("The part of the bot's account you want to edit (name, avatar)")] string action,
        //                       [Summary("The bot's new name; a direct imgur link to the bot's new avatar")] string contents)
        //{
        //    switch (action)
        //    {
        //        case "avatar":
        //            using (System.Net.WebClient wc = new System.Net.WebClient())
        //            {
        //                if (!contents.StartsWith(@"http://i.imgur.com/"))
        //                {
        //                    await ReplyAsync("Please enter a valid direct imgur link to the bot's new avatar.");
        //                    return;
        //                }
        //                else
        //                {
        //                    wc.
        //                    wc.DownloadFile(new Uri(contents), "avatar.jpg");
        //                    await Task.Delay(10);
        //                }
        //            }
        //            var avatarFile = new FileStream("avatar.jpg", FileMode.Open);
        //            await Context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Discord.API.Image(avatarFile));
        //            avatarFile.Dispose();
        //            File.Delete("avatar.jpg");
        //            break;
        //        case "name":
        //            await _client.CurrentUser.ModifyAsync(x => x.Username = contents);
        //            break;
        //    }

        //}

        [Command("info")]
        [Summary("Returns general information about the bot")]
        public async Task Info()
        {
            DateTime startTime = Process.GetCurrentProcess().StartTime;
            TimeSpan uptime = DateTime.Now.Subtract(startTime);
            StringBuilder response = new StringBuilder("```");
            response.AppendLine($"Uptime: {uptime.Days}d {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s");

            await ReplyAsync(response.ToString() + "```");
        }

        public CoreModule(DiscordSocketClient c, CommandService s, BlacklistService b, CoreConfig config)
        {
            _client = c;
            _commands = s;
            _blacklist = b;
            _config = config;
        }
    }
}
