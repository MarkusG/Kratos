using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
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

        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var service = services.GetService<PermissionsService>();
            var config = services.GetService<CoreConfig>();
            bool hasPermission = false;
            foreach (var r in (context.User as SocketGuildUser).Roles)
            {
                if (!service.Permissions.ContainsKey(r.Id)) continue; 
                if (service.Permissions[r.Id].Contains(Permission))
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
