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

        public bool EditsLogged { get; private set; }

        public bool DeletesLogged { get; private set; }

        public bool JoinsLogged { get; private set; }

        public bool LeavesLogged { get; private set; }

        public bool NameChangesLogged { get; private set; }

        public bool NickChangesLogged { get; private set; }

        #region Server Log Command Methods
        public void EnableEditLogging()
        {
            _client.MessageUpdated += _client_MessageUpdated;
            EditsLogged = true;
        }

        public void DisableEditLogging()
        {
            _client.MessageUpdated -= _client_MessageUpdated;
            EditsLogged = false;
        }

        public void EnableDeleteLogging()
        {
            _client.MessageDeleted += _client_MessageDeleted;
            DeletesLogged = true;
        }

        public void DisableDeleteLogging()
        {
            _client.MessageDeleted -= _client_MessageDeleted;
            DeletesLogged = false;
        }

        public void EnableJoinLogging()
        {
            _client.UserJoined += _client_UserJoined;
            JoinsLogged = true;
        }

        public void DisableJoinLogging()
        {
            _client.UserJoined -= _client_UserJoined;
            JoinsLogged = false;
        }

        public void EnableLeaveLogging()
        {
            _client.UserLeft += _client_UserLeft;
            LeavesLogged = true;
        }

        public void DisableLeaveLogging()
        {
            _client.UserLeft -= _client_UserLeft;
            LeavesLogged = false;
        }

        public void EnableNameChangeLogging()
        {
            _client.UserUpdated += _client_UserUpdated_NameChange;
            NameChangesLogged = true;
        }

        public void DisableNameChangeLogging()
        {
            _client.UserUpdated -= _client_UserUpdated_NameChange;
            NameChangesLogged = false;
        }

        public void EnableNickChangeLogging()
        {
            _client.GuildMemberUpdated += _client_GuildMemberUpdated_NickChange;
            NickChangesLogged = true;
        }

        public void DisableNickChangeLogging()
        {
            _client.GuildMemberUpdated -= _client_GuildMemberUpdated_NickChange;
            NickChangesLogged = false;
        }
        #endregion

        public async Task LogServerMessageAsync(string message)
        {
            if (ServerLogChannelId == 0) return;
            var channel = _client.GetChannel(ServerLogChannelId) as ITextChannel;
            await channel.SendMessageAsync(message);
        }

        public async Task LogModMessageAsync(string message)
        {
            if (ModLogChannelId == 0) return;
            var channel = _client.GetChannel(ModLogChannelId) as ITextChannel;
            await channel.SendMessageAsync(message);
        }

        public async Task<bool> SaveConfigurationAsync()
        {
            var config = new LogConfig(this);

            var serializedConfig = JsonConvert.SerializeObject(config);

            using (var configStream = File.OpenWrite(Path.Combine(Directory.GetCurrentDirectory(), "config", "log.json")))
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
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "config", "log.json"))) return false;

            using (var configStream = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "config", "log.json")))
            {
                using (var configReader = new StreamReader(configStream))
                {
                    var serializedConfig = await configReader.ReadToEndAsync();
                    var config = JsonConvert.DeserializeObject<LogConfig>(serializedConfig);
                    if (config == null) return false;

                    ServerLogChannelId = config.ServerLog;
                    ModLogChannelId = config.ModLog;
                    EditsLogged = config.EditsLogged;
                    if (EditsLogged) EnableEditLogging();
                    DeletesLogged = config.DeletesLogged;
                    if (DeletesLogged) EnableDeleteLogging();
                    JoinsLogged = config.JoinsLogged;
                    if (JoinsLogged) EnableJoinLogging();
                    LeavesLogged = config.LeavesLogged;
                    if (LeavesLogged) EnableLeaveLogging();
                    NameChangesLogged = config.NameChangesLogged;
                    if (NameChangesLogged) EnableNameChangeLogging();
                    NickChangesLogged = config.NickChangesLogged;
                    if (NickChangesLogged) EnableNickChangeLogging();

                    return true;
                }
            }
        }

        #region Server Log Event Handlers
        private async Task _client_MessageUpdated(Optional<SocketMessage> b, SocketMessage a)
        {
            var before = b.GetValueOrDefault();
            if (before == null) return;
            var author = a.Author as IGuildUser;
            if (author == null) return;
            await LogServerMessageAsync($"{author.Nickname ?? author.Username} ({author.Id}) edited their message in {(a.Channel as ITextChannel).Mention}:\n" +
                                        $"Before: `{before.Content}`\n" + 
                                        $"After: `{a.Content}`");
        }

        private async Task _client_MessageDeleted(ulong id, Optional<SocketMessage> m)
        {
            var message = m.GetValueOrDefault();
            if (message == null) return;
            var author = message.Author as IGuildUser;
            if (author == null) return;
            await LogServerMessageAsync($"{author.Nickname ?? author.Username} ({author.Id})'s message was deleted in {(message.Channel as ITextChannel).Mention}:\n" +
                                        $"`{message.Content}`");
        }

        private async Task _client_UserJoined(SocketGuildUser u)
        {
            await LogServerMessageAsync($":new: {u.Username}#{u.Discriminator} ({u.Id}) joined the server.");
        }

        private async Task _client_UserLeft(SocketGuildUser u)
        {
            await LogServerMessageAsync($":wave: {u.Username}#{u.Discriminator} ({u.Id}) left the server.");
        }

        private async Task _client_UserUpdated_NameChange(SocketUser b, SocketUser a)
        {
            if (b.Username == a.Username) return;

            await LogServerMessageAsync($"{b.Username}#{b.Discriminator} ({b.Id}) changed their username to {a.Username}");
        }

        private async Task _client_GuildMemberUpdated_NickChange(SocketGuildUser b, SocketGuildUser a)
        {
            if (b.Nickname == a.Nickname) return;

            await LogServerMessageAsync($"{b.Nickname ?? b.Username} ({b.Id}) changed their nickname to {a.Nickname}");
        }
        #endregion

        public LogService(DiscordSocketClient c)
        {
            _client = c;
        }
    }
}
