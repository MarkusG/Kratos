using System;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Kratos.Configs;

namespace Kratos.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    class RequireMasterAttribute : PreconditionAttribute
    {
        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider map)
        {
            var config = map.GetService<CoreConfig>();

            return await Task.FromResult(
                context.User.Id == config.MasterId
                ? PreconditionResult.FromSuccess()
                : PreconditionResult.FromError("Insufficient permission.")).ConfigureAwait(false);
        }
    }
}
