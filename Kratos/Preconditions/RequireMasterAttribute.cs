using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;
using Kratos.Services;

namespace Kratos.Preconditions
{
    public class RequireMasterAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var service = services.GetService<PermissionsService>();
            return Task.FromResult(
                context.User.Id == service.MasterId
                ? PreconditionResult.FromSuccess()
                : PreconditionResult.FromError(":x: This command can only be run by the bot's master."));
        }
    }
}
