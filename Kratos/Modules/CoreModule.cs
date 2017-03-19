using System.Net.Http;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Kratos.Services;
using Kratos.Configs;
using Kratos.Preconditions;
using System;

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

        [Command("gendocs"), Summary("Generates these docs")]
        [RequireCustomPermission("core.gendocs")]
        public async Task GenerateDocumentation()
        {
            StringBuilder response = new StringBuilder();
            foreach (var m in _commands.Modules)
            {
                if (m.Commands.Count() < 0) continue;
                response.AppendLine($"# {m.Name} - {m.Aliases.FirstOrDefault()} #");
                response.AppendLine();
                foreach (var c in m.Commands)
                {
                    var prefixlessAliases = c.Aliases.Select(x => x.Remove(0, c.Module.Aliases.FirstOrDefault().Length));
                    response.Append($"## `{prefixlessAliases.Aggregate((b, a) => b + " | " + a)} ");
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
                        response.AppendLine();
                    }

                    var permission = c.Preconditions.FirstOrDefault(x => x is RequireCustomPermissionAttribute) as RequireCustomPermissionAttribute;
                    if (permission != null)
                    {
                        response.AppendLine("### Permission ###");
                        response.AppendLine($"`{permission.Permission}`");
                    }

                    response.AppendLine();
                }
            }

            var helpBytes = Encoding.ASCII.GetBytes(response.ToString());
            var stream = new MemoryStream(helpBytes);

            await Context.Channel.SendFileAsync(stream, "help.md");
        }

        [Command("setmuterole"), Alias("smr")]
        [Summary("Sets the mute role")]
        [RequireCustomPermission("core.manage")]
        public async Task SetMuteRole([Summary("Self-explanatory")] SocketRole role)
        {
            _config.MuteRoleId = role.Id;
            await _config.SaveAsync();
            await ReplyAsync(":ok:");
        }

        [Command("addbypassrole"), Alias("abp")]
        [Summary("Adds a role for which the bot will ignore active protection")]
        [RequireCustomPermission("core.manage")]
        public async Task AddBypassRole([Summary("The role to add to the ignore list")] SocketRole role)
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
        public async Task RemoveBypassRole([Summary("The role to remove from the ignore list")] SocketRole role)
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

        [Command("shutdown", RunMode = RunMode.Async)]
        [Summary("Disconnects and shuts down the bot")]
        [RequireCustomPermission("core.shutdown")]
        public async Task Shutdown()
        {
            await _client.SetStatusAsync(UserStatus.Invisible);
            await ReplyAsync(":ok:");
            await _client.StopAsync();
            Environment.Exit(0);
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
