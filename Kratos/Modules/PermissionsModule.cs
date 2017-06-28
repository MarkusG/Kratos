using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Commands;
using Kratos.Preconditions;
using Kratos.Services;

namespace Kratos.Modules
{
    [Name("Permissions Module"), Group("perms")]
    [Summary("Provides commands for managing the bot's permissions system.")]
    public class PermissionsModule : ModuleBase<SocketCommandContext>
    {
        private PermissionsService _service;

        [Command("auth")]
        [Summary("Authenticates you as the master of the bot.")]
        public async Task AuthAsync([Summary("The auth code written to the console when the bot launched")] ushort code)
        {
            if (_service.AuthCode == 0)
            {
                await ReplyAsync(":x: Already authed.");
                return;
            }

            if (code == _service.AuthCode)
            {
                await _service.AuthAsync(Context.User.Id);
                await ReplyAsync(":ok:");
            }
        }

        [Command("add"), Alias("+")]
        [Summary("Adds a permission or set of permissions to a role.")]
        [Permission("permissions.manage")]
        public async Task AddAsync([Summary("Target role")] SocketRole role,
                                   [Summary("Self explanatory")] string permission)
        {
            var result = await _service.AddPermissionAsync(role.Id, permission);
            await ReplyAsync(result.ToString());
        }

        [Command("remove"), Alias("-")]
        [Summary("Removes a permission or set of permissions from a role.")]
        [Permission("permissions.manage")]
        public async Task RemoveAsyc([Summary("Target role")] SocketRole role,
                                     [Summary("Self explanatory")] string permission)
        {
            var result = await _service.RemovePermissionAsync(role.Id, permission);
            await ReplyAsync(result.ToString());
        }

        public PermissionsModule(PermissionsService p)
        {
            _service = p;
        }
    }
}
