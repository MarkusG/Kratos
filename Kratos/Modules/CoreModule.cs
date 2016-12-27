using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using Discord.WebSocket;
using Discord.Commands;

namespace Kratos.Modules
{
    [Name("Core Module")]
    [Summary("The bot's core commands.")]
    public class CoreModule : ModuleBase
    {
        private DiscordSocketClient _client;
        private CommandService _commands;

        //[Command("help"), Summary("Displays this help message")]
        //public async Task Help()
        //{
        //    StringBuilder response = new StringBuilder();
        //    response.AppendLine("Contacts Commands:");
        //    response.AppendLine();
        //    foreach (var m in _commands.Modules)
        //    {
        //        if (m.Commands.Count() < 0) continue;
        //        response.AppendLine($"{m.Name}");
        //        response.AppendLine();
        //        foreach (var c in m.Commands)
        //        {
        //            response.Append($"{c.Aliases.Aggregate((b, a) => b + " | " + a)} ");
        //            foreach (var p in c.Parameters)
        //            {
        //                if (p.IsOptional)
        //                    response.Append($"[{p.Name}] ");
        //                else
        //                    response.Append($"<{p.Name}> ");
        //            }
        //            response.AppendLine($"- {c.Summary}");
        //            foreach (var p in c.Parameters)
        //            {
        //                var optional = p.IsOptional ? "(Optional)" : null;
        //                response.AppendLine($"\t{p.Name} - {p.Summary} {optional}");
        //            }
        //            response.AppendLine();
        //        }
        //    }
        //    if (!Directory.Exists("resources"))
        //        Directory.CreateDirectory("resources");

        //    using (var helpFile = File.Create(@"resources\help.txt"))
        //    {
        //        using (var helpWriter = new StreamWriter(helpFile))
        //        {
        //            await helpWriter.WriteAsync(response.ToString());
        //        }
        //    }

        //    await Context.Channel.SendFileAsync(@"resources\help.txt");
        //}

        [Command("ping"), Summary("Returns \"Pong!\"")]
        public async Task Ping()
        {
            await ReplyAsync($"Pong! My latency is currently {_client.Latency}ms.");
        }

        //[Command("edituser"), Summary("Edits the bot's account")]
        //public async Task Edit([Summary("The part of the bot's account you want to edit (name, avatar)")] string action,
        //                       [Summary("The bot's new name; a direct imgur link to the bot's new avatar")] string contents)
        //{
        //    switch (action)
        //    {
        //        case "avatar":
        //            using (System.Net.WebClient wc = new System.Net.WebClient())
        //            {
        //                if (!contents.StartsWith(@"http://i.imgur.com/"))
        //                {
        //                    await ReplyAsync("Please enter a valid direct imgur link to the bot's new avatar.");
        //                    return;
        //                }
        //                else
        //                {
        //                    wc.
        //                    wc.DownloadFile(new Uri(contents), "avatar.jpg");
        //                    await Task.Delay(10);
        //                }
        //            }
        //            var avatarFile = new FileStream("avatar.jpg", FileMode.Open);
        //            await Context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Discord.API.Image(avatarFile));
        //            avatarFile.Dispose();
        //            File.Delete("avatar.jpg");
        //            break;
        //        case "name":
        //            await _client.CurrentUser.ModifyAsync(x => x.Username = contents);
        //            break;
        //    }

        //}

        [Command("info"), Summary("Returns general information about the bot")]
        public async Task Info()
        {
            DateTime startTime = Process.GetCurrentProcess().StartTime;
            TimeSpan uptime = DateTime.Now.Subtract(startTime);
            StringBuilder response = new StringBuilder("```");
            response.AppendLine($"Uptime: {uptime.Days}d {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s");

            await ReplyAsync(response.ToString() + "```");
        }

        public CoreModule(DiscordSocketClient c, CommandService s)
        {
            _client = c;
            _commands = s;
        }
    }
}
