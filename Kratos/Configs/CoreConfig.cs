using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Newtonsoft.Json;

namespace Kratos.Configs
{
    public class CoreConfig
    {
        public string Token { get; set; }
        public string Prefix { get; set; }
        public bool MentionPrefix { get; set; }
        public ulong MasterId { get; set; }

        public bool MentionPrefixEnabled(SocketUserMessage m, DiscordSocketClient c, ref int ap)
        {
            if (!MentionPrefix)
                return false;
            return m.HasMentionPrefix(c.CurrentUser, ref ap);
        }

        public static async Task<CoreConfig> UseCurrent()
        {
            CoreConfig result;
            using (var configStream = File.OpenRead(@"config\core.json"))
            {
                using (var configReader = new StreamReader(configStream))
                {
                    var deserializedConfig = await configReader.ReadToEndAsync();

                    result = JsonConvert.DeserializeObject<CoreConfig>(deserializedConfig);
                    return result;
                }
            }
        }

        public static async Task<CoreConfig> CreateNew()
        {
            CoreConfig result;
            result = new CoreConfig();

            Console.WriteLine("Enter your bot's token: ");
            result.Token = Console.ReadLine();
            Console.WriteLine("Enter desired command prefix: ");
            result.Prefix = Console.ReadLine();
            Console.WriteLine("Enter the master ID (will bypass all permission checks) (this will probably be your ID): ");
            result.MasterId = ulong.Parse(Console.ReadLine());
            Console.WriteLine("Allow mentioning yourself as a substitute for a command prefix? (y/n, leave blank for no): ");
            char input = Console.ReadLine().ToLower()[0];
            switch (input)
            {
                case 'y': result.MentionPrefix = true; break;
                case 'n': result.MentionPrefix = false; break;
                default: result.MentionPrefix = false; break;
            }

            using (var configStream = File.Create(@"config\core.json"))
            {
                using (var configWriter = new StreamWriter(configStream))
                {
                    var save = JsonConvert.SerializeObject(result, Formatting.Indented);
                    await configWriter.WriteAsync(save);
                }
            }
            return result;
        }
    }
}
