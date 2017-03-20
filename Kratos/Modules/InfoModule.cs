using System;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Kratos.Preconditions;
using System.Diagnostics;

namespace Kratos.Modules
{
    [Name("Info Module"), Group("info")]
    [Summary("Commands for getting information about the server, a user, or the bot itself.")]
    public class InfoModule : ModuleBase
    {
        private DiscordSocketClient _client;

        [Command("user")]
        [Summary("Get information about a user")]
        [RequireCustomPermission("info.user")]
        public async Task UserInfo([Summary("User for which to get information")] SocketGuildUser user)
        {
            var response = new EmbedBuilder()
                .WithTitle($"User information for {user.Username}#{user.Discriminator}")
                .WithThumbnailUrl(user.GetAvatarUrl())
                .AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "ID";
                    x.Value = user.Id.ToString();
                });

            if (user.Nickname != null)
            {
                response.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Nickname";
                    x.Value = user.Nickname;
                });
            }
            if (user.Game.HasValue)
            {
                response.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Game";
                    x.Value = user.Game.Value.Name;
                });
            }
            response
                .AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Status";
                    if (user.Status == UserStatus.DoNotDisturb)
                        x.Value = "Do Not Disturb";
                    else
                        x.Value = user.Status.ToString();
                })
                .AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Created at";
                    x.Value = user.CreatedAt.UtcDateTime.ToString();
                })
                .AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Joined at";
                    x.Value = user.JoinedAt.GetValueOrDefault().UtcDateTime.ToString();
                })
                .AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "Roles";
                    x.Value = string.Join(", ", user.Roles.Select(r => r.Name));
                });
            await ReplyAsync("", embed: response);
        }

        [Command("server")]
        [Summary("Get information for the current server")]
        [RequireCustomPermission("info.server")]
        public async Task ServerInfo()
        {
            var textChannels = await Context.Guild.GetTextChannelsAsync();
            var voiceChannels = await Context.Guild.GetVoiceChannelsAsync();

            var response = new EmbedBuilder()
                .WithTitle($"Server information for {Context.Guild.Name}")
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "ID";
                    x.Value = Context.Guild.Id.ToString();
                })
                .AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Created at";
                    x.Value = Context.Guild.CreatedAt.UtcDateTime.ToString();
                })
                .AddField(async x =>
                {
                    x.IsInline = true;
                    x.Name = "Default channel";
                    x.Value = (await Context.Guild.GetDefaultChannelAsync()).Mention;
                })
                .AddField(async x =>
                {
                    x.IsInline = true;
                    x.Name = "Owner";
                    x.Value = (await Context.Guild.GetOwnerAsync()).Mention;
                })
                .AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Voice region";
                    x.Value = Context.Guild.VoiceRegionId;
                })
                .AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Authentication level";
                    x.Value = Context.Guild.MfaLevel.ToString();
                })
                .AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Text channel count";
                    x.Value = textChannels.Count().ToString();
                })
                .AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Voice channel count";
                    x.Value = voiceChannels.Count().ToString();
                })
                .AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "Text channels";
                    x.Value = string.Join(", ", textChannels.Select(c => c.Name));
                })
                .AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "Voice channels";
                    x.Value = string.Join(", ", voiceChannels.Select(c => c.Name));
                })
                .AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "Roles";
                    x.Value = string.Join(", ", Context.Guild.Roles.Select(r => r.Name));
                });
            await ReplyAsync("", embed: response);
        }

        [Command("help"), Summary("Displays this help page")]
        public async Task Help() =>
            await ReplyAsync("https://github.com/MarkusGordathian/Kratos/wiki/Commands");

        [Command("ping")]
        [Summary("Returns \"Pong!\" and the bot's latency to Discord")]
        public async Task Ping()
        {
            await ReplyAsync($"Pong! My latency is currently {_client.Latency}ms.");
        }

        [Command("bot")]
        [Summary("Returns general information about the bot")]
        public async Task Info()
        {
            DateTime startTime = Process.GetCurrentProcess().StartTime;
            TimeSpan uptime = DateTime.Now.Subtract(startTime);
            StringBuilder response = new StringBuilder("```");
            response.AppendLine($"Uptime: {uptime.Days}d {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s");

            await ReplyAsync(response.ToString() + "```");
        }

        [Command("roles")]
        [Summary("Get role information for all roles on the server")]
        public async Task Roles()
        {
            var response = new StringBuilder("**ROLES**:\n");
            foreach (var r in Context.Guild.Roles.OrderByDescending(r => r.Position))
            {
                response.AppendLine($"{r.Position}. {r.Name} ({r.Id})");
            }
            await ReplyAsync(response.ToString().Replace("@everyone", "everyonerole"));
        }

        public InfoModule(DiscordSocketClient c)
        {
            _client = c;
        }
    }
}
