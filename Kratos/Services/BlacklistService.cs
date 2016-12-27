using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Discord;
using Discord.WebSocket;
using Kratos.Configs;

namespace Kratos.Services
{
    public class BlacklistService
    {
        private DiscordSocketClient _client;

        public ulong MuteRoleId { get; set; }
        public ulong LogChannelId { get; set; }
        public List<string> Blacklist { get; set; }
        public List<ulong> BypassIds { get; set; }
        public int MuteTime { get; set; }
        public bool IsEnabled { get; set; }

        public BlacklistService(DiscordSocketClient c)
        {
            _client = c;
            Blacklist = new List<string>();
            BypassIds = new List<ulong>();
            MuteTime = 360000;
        }

        public void Enable()
        {
            _client.MessageReceived += _client_MessageReceived_Blacklist;
            IsEnabled = true;
        }
        public void Disable()
        {
            _client.MessageReceived -= _client_MessageReceived_Blacklist;
            IsEnabled = false;
        }

        public async Task<bool> SaveConfigurationAsync()
        {
            var config = new BlacklistConfig
            {
                MuteRoleId = MuteRoleId,
                LogChannelId = LogChannelId,
                MuteTimeInMiliseconds = MuteTime,
                Enabled = IsEnabled,
                Blacklist = this.Blacklist.ToArray(),
                BypassIds = this.BypassIds.ToArray()
            };

            var serializedConfig = JsonConvert.SerializeObject(config, Formatting.Indented);

            using (var configStream = File.OpenWrite(@"config\blacklist.json"))
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
            if (!File.Exists(@"config\blacklist.json")) return false;

            using (var configStream = File.OpenRead(@"config\blacklist.json"))
            {
                using (var configReader = new StreamReader(configStream))
                {
                    var deserializedConfig = await configReader.ReadToEndAsync();
                    var config = JsonConvert.DeserializeObject<BlacklistConfig>(deserializedConfig);
                    if (config == null) return false;

                    MuteRoleId = config.MuteRoleId;
                    LogChannelId = config.LogChannelId;
                    MuteTime = config.MuteTimeInMiliseconds;
                    IsEnabled = config.Enabled;
                    Blacklist = config.Blacklist.ToList();
                    BypassIds = config.BypassIds.ToList();

                    return true;
                }
            }
        }

        private async Task _client_MessageReceived_Blacklist(SocketMessage m)
        {
            if (m.Author.Id == _client.CurrentUser.Id) return;
            var author = m.Author as IGuildUser;
            if (author == null) return;
            var guild = (m.Channel as IGuildChannel)?.Guild;
            if (guild == null) return;
            if (author.RoleIds.Any(x => BypassIds.Contains(x))) return;
            if (!Blacklist.Any(x => m.Content.Contains(x))) return;

            var logChannel = await guild.GetChannelAsync(LogChannelId) as ITextChannel;
            var muteRole = guild.GetRole(MuteRoleId);
            await m.DeleteAsync();
            var dmChannel = await author.GetDMChannelAsync() ?? await author.CreateDMChannelAsync();
            await dmChannel.SendMessageAsync($"You've been muted for violating the world blacklist: `{m.Content}`");
            if (logChannel != null)
                await logChannel.SendMessageAsync($"I automatically muted **{author.Nickname ?? author.Username}** for violating the word blacklist in #{m.Channel.Name}: `{m.Content}`");

            var workThread = new Thread(async (o) =>
            {
                var target = o as IGuildUser;
                await target.AddRolesAsync(muteRole);
                await Task.Delay(MuteTime);
                await target.RemoveRolesAsync(muteRole);
            });

            workThread.Start(author);
        }
    }
}
