using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Kratos.Services;
using Kratos.Configs;

namespace Kratos.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    class RequireCustomPermissionAttribute : PreconditionAttribute
    {
        public string Permission { get; set; }

        public RequireCustomPermissionAttribute(string permission)
        {
            Permission = permission;
        }

        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            var service = map.Get<PermissionsService>();
            var config = map.Get<CoreConfig>();
            bool hasPermission = false;
            foreach (var r in (context.User as IGuildUser).RoleIds)
            {
                if (!service.Permissions.ContainsKey(r)) continue; 
                if (service.Permissions[r].Contains(Permission))
                    hasPermission = true;
            }
            if (context.User.Id == config.MasterId) hasPermission = true;
            if (context.User.Id == context.Guild.OwnerId) hasPermission = true;

            return await Task.FromResult(
                hasPermission
                ? PreconditionResult.FromSuccess()
                : PreconditionResult.FromError("Insufficient permissions.")).ConfigureAwait(false);
        }
    }
}
