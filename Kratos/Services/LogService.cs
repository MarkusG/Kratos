using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Discord;
using Discord.WebSocket;
using Kratos.Configs;

namespace Kratos.Services
{
    public class LogService
    {
        private DiscordSocketClient _client;

        public ulong ServerLogChannelId { get; set; }

        public ulong ModLogChannelId { get; set; }

        public async Task LogServerMessage(string message)
        {
            var channel = _client.GetChannel(ServerLogChannelId) as ITextChannel;
            if (channel == null) return;
            await channel.SendMessageAsync(message);
        }

        public async Task LogModMessage(string message)
        {
            var channel = _client.GetChannel(ModLogChannelId) as ITextChannel;
            if (channel == null) return;
            await channel.SendMessageAsync(message);
        }

        public async Task<bool> SaveConfigurationAsync()
        {
            var config = new LogConfig
            {
                ServerLog = ServerLogChannelId,
                ModLog = ModLogChannelId
            };

            var serializedConfig = JsonConvert.SerializeObject(config);

            using (var configStream = File.OpenWrite(@"config\log.json"))
            {
                using (var configWriter = new StreamWriter(configStream))
                {
                    await configWriter.WriteAsync(serializedConfig);
                    return true;
                }
            }
        }

        public async Task<bool> LoadConfigurationAsync()
        {
            if (!File.Exists(@"config\log.json")) return false;

            using (var configStream = File.OpenRead(@"config\log.json"))
            {
                using (var configReader = new StreamReader(configStream))
                {
                    var serializedConfig = await configReader.ReadToEndAsync();
                    var config = JsonConvert.DeserializeObject<LogConfig>(serializedConfig);
                    if (config == null) return false;

                    ServerLogChannelId = config.ServerLog;
                    ModLogChannelId = config.ModLog;

                    return true;
                }
            }
        }

        public LogService(DiscordSocketClient c)
        {
            _client = c;
        }
    }
}
