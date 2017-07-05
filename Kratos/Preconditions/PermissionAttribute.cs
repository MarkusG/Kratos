using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using Discord.Commands;
using Kratos.Services;

namespace Kratos.Preconditions
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PermissionAttribute : PreconditionAttribute
    {
        public string Permission { get; set; }

        public PermissionAttribute(string permission) =>
            Permission = permission;

        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var service = services.GetService<PermissionsService>();
            if (context.User.Id == service.MasterId)
                return PreconditionResult.FromSuccess();
            var user = context.User as SocketGuildUser;

            foreach (var r in user.Roles)
            {
                if (await service.CheckPermissionsAsync(r.Id, Permission))
                    return PreconditionResult.FromSuccess();
            }

            return PreconditionResult.FromError("Insufficient permissions.");
        }
    }
}
