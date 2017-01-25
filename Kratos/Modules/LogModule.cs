using System.Threading.Tasks;
using System.Text;
using Discord;
using Discord.Commands;
using Kratos.Preconditions;
using Kratos.Services;

namespace Kratos.Modules
{
    [Name("Log Module")]
    [Group("log")]
    [Summary("Manage logging to Discord")]
    public class LogModule : ModuleBase
    {
        private LogService _log;

        [Command("modchannel")]
        [Summary("Set the mod log channel")]
        [RequireCustomPermission("log.manage")]
        public async Task SetModLogChannel([Summary("Channel to which to set the mod log")] ITextChannel channel)
        {
            _log.ModLogChannelId = channel.Id;
            await _log.SaveConfigurationAsync();
            await ReplyAsync(":ok:");
        }

        [Command("serverchannel")]
        [Summary("Set the server log channel")]
        [RequireCustomPermission("log.manage")]
        public async Task SetServerLogChannel([Summary("Channel to which to set the server log")] ITextChannel channel)
        {
            _log.ServerLogChannelId = channel.Id;
            await _log.SaveConfigurationAsync();
            await ReplyAsync(":ok:");
        }

        [Command("serverlogactions")]
        [Summary("Returns which actions are currently logged in the server log channel")]
        [RequireCustomPermission("log.view")]
        public async Task ListLogActions()
        {
            var response = new StringBuilder("**CURRENTLY LOGGED ACTIONS:**\n");
            response.AppendLine($"Message edits: {_log.EditsLogged}");
            response.AppendLine($"Message deletes: {_log.DeletesLogged}");
            response.AppendLine($"User joins: {_log.JoinsLogged}");
            response.AppendLine($"User leaves: {_log.LeavesLogged}");
            response.AppendLine($"Username changes: {_log.NameChangesLogged}");
            response.AppendLine($"Nickname changes: {_log.NickChangesLogged}");
            await ReplyAsync(response.ToString());
        }

        [Command("edits")]
        [Summary("Toggle logging message edits")]
        [RequireCustomPermission("log.manage")]
        public async Task LogEdits()
        {
            if (!_log.EditsLogged)
            {
                _log.EnableEditLogging();
                await ReplyAsync(":ok: Now logging edits.");
            }
            else
            {
                _log.DisableEditLogging();
                await ReplyAsync(":ok: No longer logging edits.");
            }

            await _log.SaveConfigurationAsync();
        }

        [Command("deletes")]
        [Summary("Toggle logging message deletes")]
        [RequireCustomPermission("log.manage")]
        public async Task LogDeletes()
        {
            if (!_log.DeletesLogged)
            {
                _log.EnableDeleteLogging();
                await ReplyAsync(":ok: Now logging deletes.");
            }
            else
            {
                _log.DisableDeleteLogging();
                await ReplyAsync(":ok: No longer logging deletes.");
            }

            await _log.SaveConfigurationAsync();
        }

        [Command("joins")]
        [Summary("Toggle logging users joining")]
        [RequireCustomPermission("log.manage")]
        public async Task LogJoins()
        {
            if (!_log.JoinsLogged)
            {
                _log.EnableJoinLogging();
                await ReplyAsync(":ok: Now logging joins.");
            }
            else
            {
                _log.DisableJoinLogging();
                await ReplyAsync(":ok: No longer logging joins.");
            }

            await _log.SaveConfigurationAsync();
        }

        [Command("leaves")]
        [Summary("Toggle logging users leaving")]
        [RequireCustomPermission("log.manage")]
        public async Task LogLeaves()
        {
            if (!_log.LeavesLogged)
            {
                _log.EnableLeaveLogging();
                await ReplyAsync(":ok: Now logging leaves.");
            }
            else
            {
                _log.DisableLeaveLogging();
                await ReplyAsync(":ok: No longer logging leaves.");
            }

            await _log.SaveConfigurationAsync();
        }

        [Command("namechanges")]
        [Summary("Toggle logging users changing usernames")]
        [RequireCustomPermission("log.manage")]
        public async Task LogNameChanges()
        {
            if (!_log.NameChangesLogged)
            {
                _log.EnableNameChangeLogging();
                await ReplyAsync(":ok: Now logging username changes.");
            }
            else
            {
                _log.DisableNameChangeLogging();
                await ReplyAsync(":ok: No longer logging username changes.");
            }

            await _log.SaveConfigurationAsync();
        }

        [Command("nickchanges")]
        [Summary("Toggle logging users changing nicknames")]
        [RequireCustomPermission("log.manage")]
        public async Task LogNickChanges()
        {
            if (!_log.NickChangesLogged)
            {
                _log.EnableNickChangeLogging();
                await ReplyAsync(":ok: Now logging nickname changes.");
            }
            else
            {
                _log.DisableNickChangeLogging();
                await ReplyAsync(":ok: No longer logging nickname changes.");
            }

            await _log.SaveConfigurationAsync();
        }

        public LogModule(LogService l)
        {
            _log = l;
        }
    }
}
