using System;
using System.Threading.Tasks;
using Discord.Commands;
using Kratos.Configs;

namespace Kratos.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    class RequireMasterAttribute : PreconditionAttribute
    {
        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            var config = map.Get<CoreConfig>();

            return await Task.FromResult(
                context.User.Id == config.MasterId
                ? PreconditionResult.FromSuccess()
                : PreconditionResult.FromError("Insufficient permission.")).ConfigureAwait(false);
        }
    }
}
