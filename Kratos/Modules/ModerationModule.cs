using System;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using Discord.Commands;
using Kratos.Preconditions;
using Kratos.Services;
using Kratos.Configs;

namespace Kratos.Modules
{
    [Name("Moderation Module"), Group("mod")]
    [Summary("A group of moderation commands")]
    public class ModerationModule : ModuleBase
    {
        private RecordService _records;
        private UnpunishService _unpunish;
        private BlacklistService _blacklist;
        private LogService _log;
        private CoreConfig _config;

        #region Banning
        [Command("pban"), Alias("perm", "perma", "permban", "permaban")]
        [Summary("Permanently bans a user from the server.")]
        [RequireCustomPermission("mod.ban")]
        public async Task PermaBan([Summary("The user to ban")] IGuildUser user,
                                   [Summary("Reason for ban")] string reason,
                                   [Summary("Number of days for which to prune the user's messages")] int pruneDays = 0)
        {
            var author = Context.User as IGuildUser;
            var authorsHighestRole = author.RoleIds.Select(x => Context.Guild.GetRole(x))
                                                   .OrderByDescending(x => x.Position)
                                                   .First();
            var usersHighestRole = user.RoleIds.Select(x => Context.Guild.GetRole(x))
                                               .OrderByDescending(x => x.Position)
                                               .First();

            if (usersHighestRole.Position >= authorsHighestRole.Position)
            {
                await ReplyAsync(":x: You cannot ban someone above or equal to you in the role hierarchy.");
                return;
            }

            await Context.Guild.AddBanAsync(user, pruneDays);
            var name = user.Nickname == null
                ? user.Username
                : $"{user.Username} (nickname: {user.Nickname})";
            await _records.AddPermaBanAsync(Context.Guild.Id, user.Id, name, Context.User.Id, DateTime.UtcNow, reason);
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
            await _records.AddPermaBanAsync(Context.Guild.Id, id, "N/A (FORCEBANNED)", Context.User.Id, DateTime.UtcNow, reason);
            _records.DisposeContext();
            var author = Context.User as IGuildUser;
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
            var ban = await _records.AddTempBanAsync(Context.Guild.Id, id, "N/A (FORCEBANNED)", Context.User.Id, DateTime.UtcNow, DateTime.UtcNow.Add(time), reason);
            _unpunish.Bans.Add(ban);
            _records.DisposeContext();
            var author = Context.User as IGuildUser;
            await _log.LogModMessageAsync($"{author.Nickname ?? author.Username} forcebanned {id} for {time} for `{reason}`");
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
        //    var author = Context.User as IGuildUser;
        //    await _log.LogModMessageAsync($"{author.Nickname ?? author.Username} forcebanned {string.Join(", ", ids)} for `{reason}`");
        //    await ReplyAsync(":ok:");
        //}

        [Command("tban"), Alias("temp", "tempban")]
        [Summary("Temporarily bans a user from the server.")]
        [RequireCustomPermission("mod.ban")]
        public async Task TempBan([Summary("The user to ban")] IGuildUser user,
                                  [Summary("The time to ban (hh:mm:ss)")] TimeSpan time,
                                  [Summary("Reason for ban")] string reason,
                                  [Summary("Number of days for which to prune the user's messages")] int pruneDays = 0)
        {
            var author = Context.User as IGuildUser;
            var authorsHighestRole = author.RoleIds.Select(x => Context.Guild.GetRole(x))
                                                   .OrderByDescending(x => x.Position)
                                                   .First();
            var usersHighestRole = user.RoleIds.Select(x => Context.Guild.GetRole(x))
                                               .OrderByDescending(x => x.Position)
                                               .First();

            if (usersHighestRole.Position >= authorsHighestRole.Position)
            {
                await ReplyAsync(":x: You cannot ban someone above or equal to you in the role hierarchy.");
                return;
            }

            await Context.Guild.AddBanAsync(user, pruneDays);
            var name = user.Nickname == null
                ? user.Username
                : $"{user.Username} (nickname: {user.Nickname})";
            var ban = await _records.AddTempBanAsync(Context.Guild.Id, user.Id, name, Context.User.Id, DateTime.UtcNow, DateTime.UtcNow.Add(time), reason);
            _records.DisposeContext();
            _unpunish.Bans.Add(ban);
            await _log.LogModMessageAsync($"{author.Nickname ?? author.Username} temp banned {user.Username} for {time} for `{reason}`");
            await ReplyAsync(":ok:");
        }

        [Command("sban"), Alias("soft", "softban")]
        [Summary("Bans a user and immediately unbans them.")]
        [RequireCustomPermission("mod.softban")]
        public async Task SoftBan([Summary("The user to softban")] IGuildUser user,
                                  [Summary("Reason for softban")] string reason,
                                  [Summary("Number of days for which to prune the user's messages")] int pruneDays = 0)
        {
            var author = Context.User as IGuildUser;
            var authorsHighestRole = author.RoleIds.Select(x => Context.Guild.GetRole(x))
                                                   .OrderByDescending(x => x.Position)
                                                   .First();
            var usersHighestRole = user.RoleIds.Select(x => Context.Guild.GetRole(x))
                                               .OrderByDescending(x => x.Position)
                                               .First();

            if (usersHighestRole.Position >= authorsHighestRole.Position)
            {
                await ReplyAsync(":x: You cannot softban someone above or equal to you in the role hierarchy.");
                return;
            }

            await Context.Guild.AddBanAsync(user, pruneDays);
            await Context.Guild.RemoveBanAsync(user);
            var name = user.Nickname == null
                ? user.Username
                : $"{user.Username} (nickname: {user.Nickname})";
            await _records.AddSoftBanAsync(Context.Guild.Id, user.Id, name, Context.User.Id, DateTime.UtcNow, reason);
            _records.DisposeContext();
            await _log.LogModMessageAsync($"{author.Nickname ?? author.Username} softbanned {name} for `{reason}`");
            await ReplyAsync(":ok:");
        }
        #endregion

        [Command("mute")]
        [Summary("Mutes a user for a given amount of time.")]
        [RequireCustomPermission("mod.mute")]
        public async Task Mute([Summary("The user to mute")] IGuildUser user,
                               [Summary("The time to mute (hh:mm:ss)")] TimeSpan time,
                               [Summary("Reason for muting")] string reason)
        {
            var author = Context.User as IGuildUser;
            var authorsHighestRole = author.RoleIds.Select(x => Context.Guild.GetRole(x))
                                                   .OrderByDescending(x => x.Position)
                                                   .First();
            var usersHighestRole = user.RoleIds.Select(x => Context.Guild.GetRole(x))
                                               .OrderByDescending(x => x.Position)
                                               .First();

            if (usersHighestRole.Position >= authorsHighestRole.Position)
            {
                await ReplyAsync(":x: You cannot mute someone above or equal to you in the role hierarchy.");
                return;
            }

            var muteRole = Context.Guild.GetRole(_config.MuteRoleId);
            await user.AddRolesAsync(muteRole);
            var mute = await _records.AddMuteAsync(Context.Guild.Id, user.Id, Context.User.Id, DateTime.UtcNow, DateTime.UtcNow.Add(time), reason);
            _records.DisposeContext();
            _unpunish.Mutes.Add(mute);
            await ReplyAsync(":ok:");
        }

        [Command("clean")]
        [Summary("Deletes a set number of messages from the channel, as well as the message calling the command.")]
        [RequireCustomPermission("mod.clean")]
        public async Task Clean([Summary("The number of messages to delete. (max 99)")] int num)
        {
            if (num > 99)
            {
                await ReplyAsync("Specified number exceeds the 99 message limit.");
                return;
            }
            var channel = Context.Channel as ITextChannel;
            var messagesToDelete = await channel.GetMessagesAsync(num + 1).Flatten();
            await channel.DeleteMessagesAsync(messagesToDelete);
            var author = Context.User as IGuildUser;
            await _log.LogModMessageAsync($"{author.Nickname ?? author.Username} cleaned {num} messages in {(Context.Channel as ITextChannel).Mention}");
        }

        public ModerationModule(RecordService r, UnpunishService u, BlacklistService b, LogService l, CoreConfig config)
        {
            _unpunish = u;
            _records = r;
            _blacklist = b;
            _log = l;
            _config = config;
        }
    }
}
