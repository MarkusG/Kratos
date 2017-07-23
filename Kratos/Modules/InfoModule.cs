using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Kratos.Preconditions;
using Kratos.Services;
using Kratos.Data;
using Humanizer;

namespace Kratos.Modules
{
    [Name("Info Module"), Group("info")]
    [Summary("Commands for getting information about the server, a user, or the bot itself.")]
    public class InfoModule : ModuleBase
    {
        private DiscordSocketClient _client;
        private AliasTrackingService _aliases;
        private RecordService _records;

        [Command("user")]
        [Summary("Get information about a user")]
        [RequireCustomPermission("info.user")]
        public async Task UserInfo([Summary("User for which to get information")] SocketGuildUser user)
        {
            var response = new EmbedBuilder()
                .WithTitle($"User information for {user.Username}#{user.Discriminator}")
                .WithThumbnailUrl(new Uri(user.GetAvatarUrl()))
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
            var usernames = await _aliases.GetUsernamesAsync(user.Id);
            if (usernames.Count() != 0)
            {
                var usernamesList = new StringBuilder();
                foreach (var u in usernames)
                    usernamesList.AppendLine($"Until {u.Until}: {u.Alias}");
                response.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "Past usernames";
                    x.Value = usernamesList.ToString();
                });
            }
            var nicknames = (await _aliases.GetNicknamesAsync(user.Id)).Where(n => n.GuildId == Context.Guild.Id);
            if (nicknames.Count() != 0)
            {
                var nicknamesList = new StringBuilder();
                foreach (var n in nicknames)
                    nicknamesList.AppendLine($"Until {n.Until}: {n.Alias}");
                response.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "Past nicknames";
                    x.Value = nicknamesList.ToString();
                });
            }
            await ReplyAsync("", embed: response);
        }

        [Command("records")]
        [Summary("Get records for a user")]
        [RequireCustomPermission("info.records")]
        public async Task Records([Summary("The type of records to get (mutes, temps, permas)")] string type,
                                  [Summary("Target user")] SocketGuildUser user)
        {
            using (var context = new RecordContext())
            {
                await context.Database.EnsureCreatedAsync();

                var mutes = context.Mutes.Where(m => m.SubjectId == user.Id);
                var temps = context.TempBans.Where(t => t.SubjectId == user.Id);
                var permas = context.PermaBans.Where(p => p.SubjectId == user.Id);

                var response = new StringBuilder("\n");
                switch (type.ToLower())
                {
                    case "mutes":
                        foreach (var m in mutes)
                            response.AppendLine($"**({m.Key}) {m.Timestamp.ToString("mm/dd/yy hh:mm:ss UTC")}:** {m.Reason}");
                        break;                    
                    case "temps":                 
                        foreach (var t in temps)  
                            response.AppendLine($"**({t.Key}) {t.Timestamp.ToString("mm/dd/yy hh:mm:ss UTC")}:** {t.Reason}");
                        break;                    
                    case "permas":                
                        foreach (var p in permas) 
                            response.AppendLine($"**({p.Key}) {p.Timestamp.ToString("mm/dd/yy hh:mm:ss UTC")}:** {p.Reason}");
                        break;
                    default:
                        await ReplyAsync(":x: Invalid type.");
                        return;
                }
                if (response.Length > 2000)
                {
                    await ReplyAsync(":x: Too many records! The message is over 2000 characters.");
                    return;
                }
                await ReplyAsync(response.ToString());
            }
        }

        [Command("records")]
        [Summary("Get info for a specific record")]
        [RequireCustomPermission("info.records")]
        public async Task Records([Summary("Record type (mutes. temps, perms)")] string type,
                                  [Summary("Record key")] int key)
        {
            using (var context = new RecordContext())
            {
                await context.Database.EnsureCreatedAsync();
                var response = new StringBuilder("\n");
                switch (type.ToLower())
                {
                    case "mutes":
                        var mute = await context.Mutes.FirstOrDefaultAsync(m => m.Key == key);
                        if (mute == null)
                        {
                            await ReplyAsync(":x: Record not found.");
                            return;
                        }
                        var moderator = await Context.Guild.GetUserAsync(mute.ModeratorId);
                        var subject = await Context.Guild.GetUserAsync(mute.SubjectId);
                        response.AppendLine("**Subject**");
                        response.AppendLine($"{subject.Username}#{subject.Discriminator} ({subject.Id})");
                        response.AppendLine("**Moderator**");
                        response.AppendLine($"{moderator.Username}#{moderator.Discriminator} ({moderator.Id})");
                        response.AppendLine("**Timestamp**");
                        response.AppendLine(mute.Timestamp.ToString("dd/MM/yy hh:mm:ss UTC"));
                        response.AppendLine("**Expiration Date/Time**");
                        response.AppendLine(mute.UnmuteAt.ToString("dd/MM/yy hh:mm:ss UTC"));
                        response.AppendLine("**Reason**");
                        response.AppendLine(mute.Reason);
                        break;
                    case "temps":
                        var temp = await context.TempBans.FirstOrDefaultAsync(t => t.Key == key);
                        if (temp == null)
                        {
                            await ReplyAsync(":x: Record not found.");
                            return;
                        }
                        moderator = await Context.Guild.GetUserAsync(temp.ModeratorId);
                        subject = await Context.Guild.GetUserAsync(temp.SubjectId);
                        response.AppendLine("**Subject**");
                        response.AppendLine($"{subject.Username}#{subject.Discriminator} ({subject.Id})");
                        response.AppendLine("**Moderator**");
                        response.AppendLine($"{moderator.Username}#{moderator.Discriminator} ({moderator.Id})");
                        response.AppendLine("**Timestamp**");
                        response.AppendLine(temp.Timestamp.ToString("dd/MM/yy hh:mm:ss UTC"));
                        response.AppendLine("**Expiration Date/Time**");
                        response.AppendLine(temp.UnbanAt.ToString("dd/MM/yy hh:mm:ss UTC"));
                        response.AppendLine("**Reason**");
                        response.AppendLine(temp.Reason);
                        break;
                    case "perms":
                        var perm = await context.PermaBans.FirstOrDefaultAsync(p => p.Key == key);
                        if (perm == null)
                        {
                            await ReplyAsync(":x: Record not found.");
                            return;
                        }
                        moderator = await Context.Guild.GetUserAsync(perm.ModeratorId);
                        subject = await Context.Guild.GetUserAsync(perm.SubjectId);
                        response.AppendLine("**Subject**");
                        response.AppendLine($"{subject.Username}#{subject.Discriminator} ({subject.Id})");
                        response.AppendLine("**Moderator**");
                        response.AppendLine($"{moderator.Username}#{moderator.Discriminator} ({moderator.Id})");
                        response.AppendLine("**Timestamp**");
                        response.AppendLine(perm.Timestamp.ToString("dd/MM/yy hh:mm:ss UTC"));
                        response.AppendLine("**Reason**");
                        response.AppendLine(perm.Reason);
                        break;
                    default:
                        await ReplyAsync(":x: Invalid format.");
                        return;
                }
                await ReplyAsync(response.ToString());
            }
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
                .WithThumbnailUrl(new Uri(Context.Guild.IconUrl))
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

        [Command("logaliases")]
        [Summary("Toggle logging of user alias changes (nicknames/usernames)")]
        [RequireCustomPermission("info.manage")]
        public async Task LogAliases()
        {
            if (_aliases.Enabled)
            {
                _aliases.Disable();
                await ReplyAsync(":ok: Logging of alias changes disabled.");
            }
            else
            {
                _aliases.Enable();
                await ReplyAsync(":ok: Logging of alias changes enabled.");
            }
            await _aliases.SaveConfigurationAsync();
        }

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
            var startTime = Process.GetCurrentProcess().StartTime;
            var uptime = DateTime.Now.Subtract(startTime).Humanize(5);
            var runtime = RuntimeInformation.FrameworkDescription;
            var libraryVersion = DiscordConfig.Version;
            StringBuilder response = new StringBuilder("**Bot Information:**\n")
                .AppendLine($"Runtime: {runtime}")
                .AppendLine($"Library: Discord.Net version {libraryVersion}")
                .AppendLine($"Uptime: {uptime}")
                .AppendLine("-")
                .AppendLine($"Guilds: {string.Join(", ", _client.Guilds)}");
                
            await ReplyAsync(response.ToString());
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

        [Command("help"), Summary("Displays this help page")]
        public async Task Help() =>
            await ReplyAsync("https://github.com/MarkusGordathian/Kratos/wiki/Commands");

        public InfoModule(DiscordSocketClient c, AliasTrackingService a, RecordService r)
        {
            _client = c;
            _aliases = a;
            _records = r;
        }
    }
}
