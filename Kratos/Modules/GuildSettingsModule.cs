using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Kratos.Configuration;
using Kratos.Preconditions;

namespace Kratos.Modules
{
    [Name("Guild Settings Module")]
    [Summary("Provides commands for managing guild-specific settings.")]
    public class GuildSettingsModule : ModuleBase<SocketCommandContext>
    {
        private GuildConfiguration _guildsConfig;

        [Command("setup")]
        [Summary("Sets up logging channels and a mute role on this guild.")]
        [RequireBotPermission(GuildPermission.ManageRoles | GuildPermission.ManageChannels)]
        [Permission("guild.setup")]
        public async Task SetupAsync([Summary("The bot's prefix for this guild"), Remainder] string prefix = null)
        {
            var guild = _guildsConfig.GetOrCreate(Context.Guild.Id);
            if (prefix != null)
                guild.Prefix = prefix;

            var modLog = await Context.Guild.CreateTextChannelAsync("mod-log");
            guild.ModLogId = modLog.Id;

            var serverLog = await Context.Guild.CreateTextChannelAsync("server-log");
            guild.ServerLogId = serverLog.Id;

            var muteRole = await Context.Guild.CreateRoleAsync("Muted");
            foreach (var channel in Context.Guild.TextChannels)
                await channel.AddPermissionOverwriteAsync(muteRole, new OverwritePermissions(sendMessages: PermValue.Deny, addReactions: PermValue.Deny));
            foreach (var channel in Context.Guild.VoiceChannels)
                await channel.AddPermissionOverwriteAsync(muteRole, new OverwritePermissions(speak: PermValue.Deny));
            guild.MuteRoleId = muteRole.Id;

            await _guildsConfig.SaveAsync();

            await ReplyAsync(":ok:");
        }

        [Command("prefix")]
        [Summary("Sets the prefix for the current guild.")]
        [Permission("guild.prefix")]
        public async Task PrefixAsync([Remainder] string prefix)
        {
            var guild = _guildsConfig.GetOrCreate(Context.Guild.Id);
            guild.Prefix = prefix;
            await _guildsConfig.SaveAsync();
            await ReplyAsync($":ok: Prefix for this guild changed to {prefix}");
        }

        [Command("modlog")]
        [Summary("Sets the mod log channel for the current guild")]
        [Permission("guild.log")]
        public async Task ModLogAsync(SocketTextChannel channel)
        {
            var guild = _guildsConfig.GetOrCreate(Context.Guild.Id);
            guild.ModLogId = channel.Id;
            await _guildsConfig.SaveAsync();
            await ReplyAsync($":ok: Mod log changed to {channel.Mention}");
        }

        [Command("serverlog")]
        [Summary("Sets the mod log channel for the current guild")]
        [Permission("guild.log")]
        public async Task ServerLogAsync(SocketTextChannel channel)
        {
            var guild = _guildsConfig.GetOrCreate(Context.Guild.Id);
            guild.ServerLogId = channel.Id;
            await _guildsConfig.SaveAsync();
            await ReplyAsync($":ok: Server log changed to {channel.Mention}");
        }

        [Command("muterole")]
        [Summary("Sets the mute role for the current guild")]
        [Permission("guild.muterole")]
        public async Task MuteRoleAsync(SocketRole role)
        {
            var guild = _guildsConfig.GetOrCreate(Context.Guild.Id);
            guild.MuteRoleId = role.Id;
            await _guildsConfig.SaveAsync();
            await ReplyAsync($":ok: Mute role changed to {role.Name}");
        }

        public GuildSettingsModule(GuildConfiguration guilds)
        {
            _guildsConfig = guilds;
        }
    }
}
