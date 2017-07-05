using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Commands;
using Kratos.Preconditions;
using Kratos.Services;
using Kratos.Results;

namespace Kratos.Modules
{
    [Name("Permissions Module"), Group("perms")]
    [Summary("Provides commands for managing the bot's permissions system.")]
    public class PermissionsModule : ModuleBase<SocketCommandContext>
    {
        private PermissionsService _service;

        [Command("auth")]
        [Summary("Authenticates you as the master of the bot.")]
        public async Task<RuntimeResult> AuthAsync([Summary("The auth code written to the console when the bot launched")] ushort code)
        {
            if (_service.AuthCode == 0)
                return new SimpleRuntimeResult(CommandError.Unsuccessful, "Already authed.");

            if (code == _service.AuthCode)
            {
                await _service.AuthAsync(Context.User.Id);
                return new SimpleRuntimeResult(null, "Successfully authed.");
            }
            return new SimpleRuntimeResult(CommandError.Unsuccessful, "Invalid auth code.");
        }

        [Command("add"), Alias("+")]
        [Summary("Adds a permission or set of permissions to a role.")]
        [Permission("permissions.manage")]
        public async Task<RuntimeResult> AddAsync([Summary("Target role")] SocketRole role,
                                                            [Summary("Self explanatory")] string permission)
        {
            var result = await _service.AddPermissionAsync(role.Id, permission);
            return PermissionRuntimeResult.FromInnerResult(result);
        }

        [Command("remove"), Alias("-")]
        [Summary("Removes a permission or set of permissions from a role.")]
        [Permission("permissions.manage")]
        public async Task<RuntimeResult> RemoveAsync([Summary("Target role")] SocketRole role,
                                                               [Summary("Self explanatory")] string permission)
        {
            var result = await _service.RemovePermissionAsync(role.Id, permission);
            return PermissionRuntimeResult.FromInnerResult(result);
        }

        [Command("list")]
        [Summary("List permisisons for a given role")]
        [Permission("permissions.view")]
        public async Task<RuntimeResult> ListAsync([Summary("Target role")] SocketRole role)
        {
            var permissions = await _service.GetPermissionsAsync(role.Id);
            if (permissions == null)
                return new SimpleRuntimeResult(CommandError.Unsuccessful, "No permissions found for role.");
            var response = new StringBuilder($"**Permissions for {role.Name}:**\n");
            foreach (var p in permissions)
                response.AppendLine(p);
            await ReplyAsync(response.ToString());
            return new SimpleRuntimeResult(null, null);
        }

        [Command("listall")]
        [Summary("List all existing permissions")]
        [Permission("permissions.view")]
        public async Task<RuntimeResult> ListAllAsync()
        {
            var response = new StringBuilder("**All permissions:**\n");
            foreach (var p in _service.AllPermissions.OrderBy(x => x))
                response.AppendLine(p);
            await ReplyAsync(response.ToString());
            return new SimpleRuntimeResult(null, null);
        }

        public PermissionsModule(PermissionsService p)
        {
            _service = p;
        }
    }
}
