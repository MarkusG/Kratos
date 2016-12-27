using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Kratos.Modules
{
    [Summary("Fun Module")]
    public class FunModule : ModuleBase
    {
        [Command("choose"), Summary("Randomly selects from a number of options")]
        public async Task Choose([Summary("A list of choices delimited by vertical bars (ex. \"choice1 | choice2\")"), Remainder()] string args)
        {
            string[] argsArray = args.Split(new string[] { " | " }, StringSplitOptions.RemoveEmptyEntries);
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
    }
}
