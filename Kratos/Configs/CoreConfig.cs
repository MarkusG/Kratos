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
        public ulong MuteRoleId { get; set; }
        public List<ulong> BypassIds { get; set; }

        public bool MentionPrefixEnabled(SocketUserMessage m, DiscordSocketClient c, ref int ap)
        {
            if (!MentionPrefix)
                return false;
            return m.HasMentionPrefix(c.CurrentUser, ref ap);
        }

        public async Task SaveAsync()
        {
            using (var configStream = File.OpenWrite(Program.ConfigDirectory + @"core.json"))
            {
                using (var configWriter = new StreamWriter(configStream))
                {
                    var save = JsonConvert.SerializeObject(this);
                    await configWriter.WriteAsync(save);
                }
            }
        }

        public static async Task<CoreConfig> UseCurrentAsync()
        {
            CoreConfig result;
            using (var configStream = File.OpenRead(Program.ConfigDirectory + @"core.json"))
            {
                using (var configReader = new StreamReader(configStream))
                {
                    var deserializedConfig = await configReader.ReadToEndAsync();

                    result = JsonConvert.DeserializeObject<CoreConfig>(deserializedConfig);
                    if (result.BypassIds == null)
                        result.BypassIds = new List<ulong>();
                    return result;
                }
            }
        }

        public static async Task<CoreConfig> CreateNewAsync()
        {
            CoreConfig result;
            result = new CoreConfig();

            Console.WriteLine("Enter your bot's token: ");
            result.Token = Console.ReadLine();
            Console.WriteLine("Enter desired command prefix: ");
            result.Prefix = Console.ReadLine();
            Console.WriteLine("Enter the master ID (will bypass all permission checks) (this will probably be your ID): ");
            result.MasterId = ulong.Parse(Console.ReadLine());
            Console.WriteLine("Enter the mute role ID: ");
            result.MuteRoleId = ulong.Parse(Console.ReadLine());
            Console.WriteLine("Allow mentioning the bot as a substitute for a command prefix? (y/n, leave blank for no): ");
            char input = Console.ReadLine().ToLower()[0];
            result.BypassIds = new List<ulong>();
            switch (input)
            {
                case 'y': result.MentionPrefix = true; break;
                case 'n': result.MentionPrefix = false; break;
                default: result.MentionPrefix = false; break;
            }

            string directory = Directory.GetCurrentDirectory();

            using (var configStream = File.Create(Program.ConfigDirectory + @"core.json"))
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
