using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Kratos.Preconditions;

namespace Kratos.Modules
{
    [Name("Miscellaneous Module")]
    [Summary("A group of miscellaneous commands.")]
    public class MiscModule : ModuleBase
    {
        private static string _welcomeMessage; // I'm just duct taping this version together so it works(tm) 
        private DiscordSocketClient _client;

        [Command("choose")]
        [Summary("Randomly selects from a number of options")]
        [RequireCustomPermission("misc.choose")]
        public async Task Choose([Summary("A list of choices delimited by vertical bars (ex. \"choice1 | choice2\")"), Remainder] string args)
        {
            var argsArray = args.Split(new string[] { " | " }, StringSplitOptions.RemoveEmptyEntries);
            Random rand = new Random();
            int choice = rand.Next(0, argsArray.Length);
            if (argsArray.Any(x => x.Contains("@everyone")) || argsArray.Any(x => x.Contains("@here")))
            {
                await ReplyAsync(":star: you tried.");
                return;
            }
            else
                await ReplyAsync(argsArray[choice]);
        }

        [Command("welcome")]
        [Summary("Configure automatic welcome messages")]
        [RequireCustomPermission("misc.welcome")]
        public async Task Welcome([Summary("Welcome message (leave blank to disable)"), Remainder] string message = null)
        {
            if (message == null)
            {
                _client.UserJoined -= UserJoined_Welcome;
                _welcomeMessage = null;
                await ReplyAsync(":ok: Welcome message disabled.");
                return;
            }
            else
            {
                if (message == _welcomeMessage)
                {
                    await ReplyAsync("Message already set.");
                    return;
                }
                else
                {
                    if (_welcomeMessage == null)
                        _client.UserJoined += UserJoined_Welcome;
                    _welcomeMessage = message;
                    await ReplyAsync(":ok:");
                    return;
                }
            }
        }

        private static async Task UserJoined_Welcome(SocketGuildUser user) =>
            await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync(_welcomeMessage.Replace("{u}", user.Username).Replace("{g}", user.Guild.Name));

        public MiscModule(DiscordSocketClient client)
        {
            _client = client;
        }
    }
}
