using System;
using System.Net.Http;
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
        private string[] _imageExtensions = new string[]
        {
            ".jpg",
            ".jpeg",
            ".png"
        };

        [Command("help"), Summary("Displays this help page")]
        public async Task Help() =>
            await ReplyAsync("https://github.com/MarkusGordathian/Kratos/wiki/Commands");

        [Command("gendocs"), Summary("Generates these docs")]
        [RequireCustomPermission("core.gendocs")]
        public async Task GenerateDocumentation()
        {
            StringBuilder response = new StringBuilder();
            foreach (var m in _commands.Modules)
            {
                if (m.Commands.Count() < 0) continue;
                response.AppendLine($"# {m.Name} - GROUP PREFIX HERE #");
                response.AppendLine();
                foreach (var c in m.Commands)
                {
                    response.Append($"## `{c.Aliases.Aggregate((b, a) => b + " | " + a)} ");
                    foreach (var p in c.Parameters)
                    {
                        if (p.IsOptional)
                            response.Append($"[{p.Name}] ");
                        else
                            response.Append($"<{p.Name}> ");
                    }
                    response.AppendLine("` ##");
                    response.AppendLine("### Functionality ###");
                    response.AppendLine(c.Summary);
                    if (c.Parameters.Count > 0)
                    {
                        response.AppendLine("### Parameters ###");

                        foreach (var p in c.Parameters)
                        {
                            var optional = p.IsOptional ? " (Optional)" : null;
                            response.AppendLine($"* {p.Name} - {p.Summary}{optional}");
                        }
                    }

                    response.AppendLine();
                }
            }
            if (!Directory.Exists("resources"))
                Directory.CreateDirectory("resources");

            using (var helpFile = File.Create(@"resources\help.txt"))
            {
                using (var helpWriter = new StreamWriter(helpFile))
                {
                    await helpWriter.WriteAsync(response.ToString());
                }
            }

            await Context.Channel.SendFileAsync(@"resources\help.txt");
        }

        [Command("ping")]
        [Summary("Returns \"Pong!\" and the bot's latency to Discord")]
        public async Task Ping()
        {
            await ReplyAsync($"Pong! My latency is currently {_client.Latency}ms.");
        }

        [Command("setmuterole"), Alias("smr")]
        [Summary("Sets the mute role")]
        [RequireCustomPermission("core.manage")]
        public async Task SetMuteRole([Summary("Self-explanatory")] IRole role)
        {
            _config.MuteRoleId = role.Id;
            await _config.SaveAsync();
            await ReplyAsync(":ok:");
        }

        [Command("addbypassrole"), Alias("abp")]
        [Summary("Adds a role for which the bot will ignore active protection")]
        [RequireCustomPermission("core.manage")]
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
        [RequireCustomPermission("core.manage")]
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
        [RequireCustomPermission("core.view")]
        public async Task ListBypassRoles()
        {
            StringBuilder response = new StringBuilder("**BYPASS ROLES:**\n");
            foreach (var i in _config.BypassIds)
                response.AppendLine(Context.Guild.GetRole(i).Name);
            await ReplyAsync(response.ToString());
        }

        [Command("avatar")]
        [Summary("Sets the bot's avatar")]
        [RequireCustomPermission("core.modifyuser")]
        public async Task Avatar([Summary("A direct image link to the bot's new avatar")] string content)
        {
            if (!_imageExtensions.Any(x => content.EndsWith(x)))
            {
                await ReplyAsync(":x: Please enter a valid direct image link to the bot's new avatar.");
                return;
            }
            using (HttpClient httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(content);
                var image = await response.Content.ReadAsStreamAsync();
                await Context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image(image));
            }
        }

        [Command("username")]
        [Summary("Sets the bot's username")]
        [RequireCustomPermission("core.modifyuser")]
        public async Task Username([Summary("The bot's new username")] string content)
        {
            await _client.CurrentUser.ModifyAsync(x => x.Username = content);
            await ReplyAsync(":ok:");
        }

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
