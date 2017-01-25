using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Discord;
using Discord.Commands;
using Kratos.Preconditions;
using Kratos.Services;
using Kratos.Services.Results;

namespace Kratos.Modules
{
    [Name("Permissions Module"), Group("perms")]
    [Summary("Manage the bot's permission system.")]
    public class PermissionsModule : ModuleBase
    {
        private PermissionsService _service;

        [Command("addperm"), Alias("+")]
        [Summary("Add a permission to a role.")]
        [RequireCustomPermission("permissions.manage")]
        public async Task AddPerm([Summary("Role to which to add the permission")] IRole role,
                                  [Summary("Permission to be added")] string permission)
        {
            var result = _service.AddPermission(role, permission);
            switch (result.Type)
            {
                case ResultType.Success:
                    await _service.SaveConfigurationAsync();
                    await ReplyAsync(":ok: Permission added successfully.");
                    break;
                case ResultType.Warning:
                    await ReplyAsync($":warning: {result.Info}");
                    break;
                case ResultType.Fail:
                    await ReplyAsync($":x: {result.Info}");
                    break;
            }
        }

        [Command("removeperm"), Alias("-")]
        [Summary("Removes a permission from a role.")]
        [RequireCustomPermission("permissions.manage")]
        public async Task RemovePerm([Summary("Role from which to remove the permission")] IRole role,
                                     [Summary("Permission to removed")] string permission)
        {
            var result = _service.RemovePermission(role, permission);
            switch (result.Type)
            {
                case ResultType.Success:
                    await _service.SaveConfigurationAsync();
                    await ReplyAsync(":ok: Permission removed successfully.");
                    break;
                case ResultType.Warning:
                    await ReplyAsync($":warning: {result.Info}");
                    break;
                case ResultType.Fail:
                    await ReplyAsync($":x: {result.Info}");
                    break;
            }
        }

        [Command("list")]
        [Summary("Lists all permissions held by a role.")]
        [RequireCustomPermission("permissions.view")]
        public async Task ListPerms([Summary("Role for which to list permissions")] IRole role)
        {
            var response = new StringBuilder($"__Permissions held by {role.Name}__\n\n");
            foreach (var p in _service.Permissions[role.Id])
                response.AppendLine(p);
            await ReplyAsync(response.ToString());
        }

        [Command("listall")]
        [Summary("Lists all existing permissions.")]
        [RequireCustomPermission("permissions.view")]
        public async Task ListAll()
        {
            var response = new StringBuilder("__All permissions__\n\n");
            foreach (var p in _service.AllPermissions.OrderBy(x => x))
                response.AppendLine(p);
            await ReplyAsync(response.ToString());
        }

        [Command("saveconfig"), Alias("save")]
        [Summary("Saves the current permission configuration")]
        [RequireCustomPermission("permissions.manage")]
        public async Task SaveConfig()
        {
            var success = await _service.SaveConfigurationAsync();
            await ReplyAsync(success ? ":ok:" : ":x: Failed to save config.");
        }

        [Command("reloadconfig"), Alias("reload")]
        [Summary("Reloads the permission configuration from config\\permissions.json")]
        [RequireCustomPermission("permissions.manage")]
        public async Task ReloadConfig()
        {
            var success = await _service.LoadConfigurationAsync();
            await ReplyAsync(success ? ":ok:" : ":x: Failed to reload config. Please configure permissions and save the config.");
        }

        public PermissionsModule(PermissionsService p)
        {
            _service = p;
        }
    }
}
