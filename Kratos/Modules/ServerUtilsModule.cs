using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Kratos.Preconditions;

namespace Kratos.Modules
{
    [Summary("Server Utilities Module")]
    public class ServerUtilities : ModuleBase
    {
        [Command("clean"),
         Summary("Deletes a set number of messages from the channel, as well as the message calling the command."),
         RequireCustomPermission("serverutils.clean")]
        public async Task Prune([Summary("The number of messages to delete. (max 100)")] int num)
        {
            if (num > 100)
            {
                await ReplyAsync("Specified number exceeds the 100 message limit.");
                return;
            }
            var channel = Context.Channel as ITextChannel;
            var messagesToDelete = await channel.GetMessagesAsync(num).Flatten();
            await channel.DeleteMessagesAsync(messagesToDelete);
        }
    }
}
