using System;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Kratos.Preconditions;
using Kratos.Services;
using Kratos.Configs;
using Kratos.Data;
using Humanizer;

namespace Kratos.Modules
{
    [Name("Moderation Module")]
    [Summary("A group of moderation commands")]
    public class ModerationModule : ModuleBase
    {
        private ModerationService _service;
        private RecordService _records;
        private UnpunishService _unpunish;
        private BlacklistService _blacklist;
        private LogService _log;
        private CoreConfig _config;

        #region Banning
        [Command("pban"), Alias("perm", "perma", "permban", "permaban")]
        [Summary("Permanently bans a user from the server.")]
        [RequireCustomPermission("mod.ban")]
        public async Task PermaBan([Summary("The user to ban")] SocketGuildUser user,
                                   [Summary("Reason for ban")] string reason,
                                   [Summary("Number of days for which to prune the user's messages")] int pruneDays = 0)
        {
            var author = Context.User as SocketGuildUser;
            var authorsHighestRole = author.Roles.OrderByDescending(x => x.Position).First();
            var usersHighestRole = user.Roles.OrderByDescending(x => x.Position).First();

            if (usersHighestRole.Position >= authorsHighestRole.Position)
            {
                await ReplyAsync(":x: You cannot ban someone above or equal to you in the role hierarchy.");
                return;
            }

            if (_service.PermaBanMessage != null && _service.UnmuteMessage != "")
            {
                var dmChannel = await user.GetOrCreateDMChannelAsync();
                await dmChannel.SendMessageAsync(_service.PermaBanMessage.Replace("{g}", user.Guild.Name)
                                                                         .Replace("{r}", reason));
            }

            await Context.Guild.AddBanAsync(user, pruneDays);
            var name = user.Nickname == null
                ? user.Username
                : $"{user.Username} (nickname: {user.Nickname})";
            await _records.AddPermaBanAsync(new PermaBan
            {
                GuildId = Context.Guild.Id,
                SubjectId = user.Id,
                SubjectName = user.Username,
                ModeratorId = Context.User.Id,
                Timestamp = DateTime.UtcNow,
                Reason = reason
            });
            _records.DisposeContext();
            await _log.LogModMessageAsync($"{author.Nickname ?? author.Username} permabanned {name} for `{reason}`");
            await ReplyAsync(":ok:");
        }

        [Command("fban"), Alias("force", "forceban")]
        [Summary("Bans a user from the server when the user is not present in the server.")]
        [RequireCustomPermission("mod.ban")]
        public async Task ForceBan([Summary("The ID to ban")] ulong id,
                                   [Summary("Reason for ban")] string reason = "N/A")
        {
            await Context.Guild.AddBanAsync(id);
            await _records.AddPermaBanAsync(new PermaBan
            {
                GuildId = Context.Guild.Id,
                SubjectId = id,
                SubjectName = "N/A (FORCEBANNED)",
                ModeratorId = Context.User.Id,
                Timestamp = DateTime.UtcNow,
                Reason = reason
            });
            _records.DisposeContext();
            var author = Context.User as SocketGuildUser;
            await _log.LogModMessageAsync($"{author.Nickname ?? author.Username} forcebanned {id} for `{reason}`");
            await ReplyAsync(":ok:");
        }

        [Command("fban"), Alias("force", "forceban")]
        [Summary("Temporarily bans a user from the server when the user is not present in the server.")]
        [RequireCustomPermission("mod.ban")]
        public async Task ForceBan([Summary("The ID to ban")] ulong id,
                                   [Summary("Time of ban")] TimeSpan time,
                                   [Summary("Reason for ban")] string reason = "N/A")
        {
            await Context.Guild.AddBanAsync(id);
            var ban = await _records.AddTempBanAsync(new TempBan
            {
                GuildId = Context.Guild.Id,
                SubjectId = id,
                SubjectName = "N/A (FORCEBANNED)",
                ModeratorId = Context.User.Id,
                Timestamp = DateTime.UtcNow,
                UnbanAt = DateTime.UtcNow.Add(time),
                Reason = reason,
                Active = true
            });
            _unpunish.Bans.Add(ban);
            _records.DisposeContext();
            var author = Context.User as SocketGuildUser;
            await _log.LogModMessageAsync($"{author.Nickname ?? author.Username} forcebanned {id} for {time.Humanize(5)} for `{reason}`");
            await ReplyAsync(":ok:");
        }

        //[Command("fban"), Alias("force", "forceban")]
        //[Summary("Bans a group of users by ID")]
        //[RequireCustomPermission("mod.ban")]
        //public async Task ForceBan([Summary("Reason for bans")] string reason,
        //                           [Summary("IDs to ban (separated by spaces)")] params ulong[] ids)
        //{
        //    if (ids.Any(x => Context.Guild.GetUserAsync(x).Result != null))
        //    {
        //        await ReplyAsync("One or more users exists in server. Please use `pban` instead.");
        //        return;
        //    }

        //    foreach (var id in ids)
        //    {
        //        await Context.Guild.AddBanAsync(id);
        //        await _records.AddPermaBanAsync(Context.Guild.Id, id, "N/A (FORCEBANNED)", Context.User.Id, (ulong)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds, reason);
        //    }
        //    _records.DisposeContext();
        //    var author = Context.User as SocketGuildUser;
        //    await _log.LogModMessageAsync($"{author.Nickname ?? author.Username} forcebanned {string.Join(", ", ids)} for `{reason}`");
        //    await ReplyAsync(":ok:");
        //}

        [Command("tban"), Alias("temp", "tempban")]
        [Summary("Temporarily bans a user from the server.")]
        [RequireCustomPermission("mod.ban")]
        public async Task TempBan([Summary("The user to ban")] SocketGuildUser user,
                                  [Summary("The time to ban (hh:mm:ss)")] TimeSpan time,
                                  [Summary("Reason for ban")] string reason,
                                  [Summary("Number of days for which to prune the user's messages")] int pruneDays = 0)
        {
            var author = Context.User as SocketGuildUser;
            var authorsHighestRole = author.Roles.OrderByDescending(x => x.Position).First();
            var usersHighestRole = user.Roles.OrderByDescending(x => x.Position).First();

            if (usersHighestRole.Position >= authorsHighestRole.Position)
            {
                await ReplyAsync(":x: You cannot ban someone above or equal to you in the role hierarchy.");
                return;
            }

            if (_service.TempBanMessage != null && _service.UnmuteMessage != "")
            {
                var dmChannel = await user.GetOrCreateDMChannelAsync();
                await dmChannel.SendMessageAsync(_service.TempBanMessage.Replace("{g}", user.Guild.Name)
                                                                        .Replace("{t}", time.Humanize(5))
                                                                        .Replace("{r}", reason));
            }
            
            await Context.Guild.AddBanAsync(user, pruneDays);
            var name = user.Nickname == null
                ? user.Username
                : $"{user.Username} (nickname: {user.Nickname})";
            var ban = await _records.AddTempBanAsync(new TempBan
            {
                GuildId = Context.Guild.Id,
                SubjectId = user.Id,
                SubjectName = name,
                ModeratorId = Context.User.Id,
                Timestamp = DateTime.UtcNow,
                UnbanAt = DateTime.UtcNow.Add(time),
                Reason = reason,
                Active = true
            });
            _records.DisposeContext();
            _unpunish.Bans.Add(ban);
            await _log.LogModMessageAsync($"{author.Nickname ?? author.Username} temp banned {user.Username} for {time.Humanize(5)} for `{reason}`");
            await ReplyAsync(":ok:");
        }

        [Command("sban"), Alias("soft", "softban")]
        [Summary("Bans a user and immediately unbans them.")]
        [RequireCustomPermission("mod.softban")]
        public async Task SoftBan([Summary("The user to softban")] SocketGuildUser user,
                                  [Summary("Reason for softban")] string reason,
                                  [Summary("Number of days for which to prune the user's messages")] int pruneDays = 0)
        {
            var author = Context.User as SocketGuildUser;
            var authorsHighestRole = author.Roles.OrderByDescending(x => x.Position).First();
            var usersHighestRole = user.Roles.OrderByDescending(x => x.Position).First();

            if (usersHighestRole.Position >= authorsHighestRole.Position)
            {
                await ReplyAsync(":x: You cannot softban someone above or equal to you in the role hierarchy.");
                return;
            }

            if (_service.SoftBanMessage != null && _service.UnmuteMessage != "")
            {
                var dmChannel = await user.GetOrCreateDMChannelAsync();
                await dmChannel.SendMessageAsync(_service.SoftBanMessage.Replace("{g}", user.Guild.Name)
                                                                        .Replace("{r}", reason));
            }

            await Context.Guild.AddBanAsync(user, pruneDays);
            await Context.Guild.RemoveBanAsync(user);
            var name = user.Nickname == null
                ? user.Username
                : $"{user.Username} (nickname: {user.Nickname})";
            await _records.AddSoftBanAsync(new SoftBan
            {
                GuildId = Context.Guild.Id,
                SubjectId = user.Id,
                SubjectName = user.Username,
                ModeratorId = Context.User.Id,
                Timestamp = DateTime.UtcNow,
                Reason = reason
            });
            _records.DisposeContext();
            await _log.LogModMessageAsync($"{author.Nickname ?? author.Username} softbanned {name} for `{reason}`");
            await ReplyAsync(":ok:");
        }
        #endregion

        [Command("mute")]
        [Summary("Mutes a user for a given amount of time.")]
        [RequireCustomPermission("mod.mute")]
        public async Task Mute([Summary("The user to mute")] SocketGuildUser user,
                               [Summary("The time to mute (hh:mm:ss)")] TimeSpan time,
                               [Summary("Reason for muting")] string reason)
        {
            var author = Context.User as SocketGuildUser;
            var authorsHighestRole = author.Roles.OrderByDescending(x => x.Position).First();
            var usersHighestRole = user.Roles.OrderByDescending(x => x.Position).First();

            if (usersHighestRole.Position >= authorsHighestRole.Position)
            {
                await ReplyAsync(":x: You cannot mute someone above or equal to you in the role hierarchy.");
                return;
            }

            if (_service.MuteMessage != null && _service.UnmuteMessage != "")
            {
                var dmChannel = await user.GetOrCreateDMChannelAsync();
                await dmChannel.SendMessageAsync(_service.MuteMessage.Replace("{g}", user.Guild.Name)
                                                                     .Replace("{t}", time.Humanize(5))
                                                                     .Replace("{r}", reason));
            }

            var muteRole = Context.Guild.GetRole(_config.MuteRoleId);
            await user.AddRoleAsync(muteRole);
            var mute = await _records.AddMuteAsync(new Mute
            {
                GuildId = Context.Guild.Id,
                SubjectId = user.Id,
                ModeratorId = Context.User.Id,
                Timestamp = DateTime.UtcNow,
                UnmuteAt = DateTime.UtcNow.Add(time),
                Reason = reason,
                Active = true
            });
            _records.DisposeContext();
            _unpunish.Mutes.Add(mute);
            await ReplyAsync(":ok:");
        }

        [Command("unmute")]
        [Summary("Unmutes a user")]
        [RequireCustomPermission("mod.mute")]
        public async Task Unmute([Summary("The user to unmute")] SocketGuildUser user)
        {
            var author = Context.User as SocketGuildUser;
            var authorsHighestRole = author.Roles.OrderByDescending(x => x.Position).First();
            var usersHighestRole = user.Roles.OrderByDescending(x => x.Position).First();

            if (usersHighestRole.Position >= authorsHighestRole.Position)
            {
                await ReplyAsync(":x: You cannot unmute someone above or equal to you in the role hierarchy.");
                return;
            }

            if (_service.UnmuteMessage != null && _service.UnmuteMessage != "")
            {
                var dmChannel = await user.GetOrCreateDMChannelAsync();
                await dmChannel.SendMessageAsync(_service.UnmuteMessage.Replace("{g}", user.Guild.Name));
            }

            var muteRole = Context.Guild.GetRole(_config.MuteRoleId);
            await user.RemoveRoleAsync(muteRole);
            await _records.DeactivateMutesForUserAsync(user.Id);
            _records.DisposeContext();
            _unpunish.Mutes.RemoveAll(x => x.SubjectId == user.Id);
            await ReplyAsync(":ok:");
        }

        [Command("clean", RunMode = RunMode.Async)]
        [Summary("Deletes a set number of messages from the channel, as well as the message calling the command.")]
        [RequireCustomPermission("mod.clean")]
        public async Task Clean([Summary("The number of messages to delete")] int amount)
        {
            var channel = Context.Channel as SocketTextChannel;
            var toDelete = channel.GetMessagesAsync(amount);
                await toDelete.ForEachAsync(async x =>
                {
                    try
                    {
                        await channel.DeleteMessagesAsync(x);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        await ReplyAsync(":warning: Some messages older than 2 weeks. Specify a lesser amount.");
                        return;
                    }
                });
            await Context.Message.DeleteAsync();
        }

        [Command("config")]
        [Summary("Configure moderation-related features.")]
        [RequireCustomPermission("mod.config")]
        public async Task Config([Summary("The setting you want to edit")] string setting,
                                 [Summary("The new value for the setting")] string value = null)
        {
            switch (setting.ToLower())
            {
                case "tempmessage": _service.TempBanMessage = value; break;
                case "permamessage": _service.PermaBanMessage = value; break;
                case "mutemessage": _service.MuteMessage = value; break;
                case "unmutemessage": _service.UnmuteMessage = value; break;
                case "softbanmessage": _service.SoftBanMessage = value; break;
                default:
                    await ReplyAsync($"Invalid setting: `{setting}`");
                    return;
            }
            await _service.SaveConfigurationAsync();
            await ReplyAsync(":ok:");
        }

        public ModerationModule(RecordService r, UnpunishService u, BlacklistService b, LogService l, CoreConfig config, ModerationService m)
        {
            _service = m;
            _unpunish = u;
            _records = r;
            _blacklist = b;
            _log = l;
            _config = config;
        }
    }
}
