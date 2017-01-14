using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.WebSocket;
using Kratos.Configs;

namespace Kratos.Services
{
    public class SlowmodeService // TODO make this channel specific
    {
        private DiscordSocketClient _client;
        private LogService _log;
        private Dictionary<ulong, DateTime> _lastmessage;
        private CoreConfig _config;

        public int IntervalInSeconds { get; set; }
        public bool IsEnabled { get; private set; }

        public void Enable(int interval)
        {
            IntervalInSeconds = interval;
            _client.MessageReceived += _client_MessageReceived_Slowmode;
            IsEnabled = true;
        }

        public void Disable()
        {
            _client.MessageReceived -= _client_MessageReceived_Slowmode;
            _lastmessage = null;
            IsEnabled = false;
        }

        private async Task _client_MessageReceived_Slowmode(SocketMessage m)
        {
            if (m.Author.Id == _client.CurrentUser.Id) return;
            var message = m as SocketUserMessage;
            var author = m.Author as IGuildUser;
            if (message == null) return;
            if (!(message.Channel is IGuildChannel)) return;
            var guild = (m.Channel as IGuildChannel).Guild;

            if (author.RoleIds.Any(x => _config.BypassIds.Contains(x))) return;

            if (_lastmessage.ContainsKey(message.Author.Id))
            {
                if (DateTime.UtcNow.Subtract(_lastmessage[message.Author.Id]).TotalSeconds >= IntervalInSeconds)
                    _lastmessage[message.Author.Id] = DateTime.UtcNow;
                else
                {
                    await message.DeleteAsync();
                    var name = author.Nickname == null
                        ? author.Username
                        : $"{author.Username} (nickname: {author.Nickname})";
                    await _log.LogModMessageAsync($"Automatically deleted {name}'s message in {(m.Channel as ITextChannel).Mention} for violating slowmode: `{m.Content}`");
                }
            }
            else
            {
                _lastmessage.Add(message.Author.Id, DateTime.UtcNow);
            }
        }

        public SlowmodeService(DiscordSocketClient c, LogService l, CoreConfig config)
        {
            _client = c;
            _log = l;
            _config = config;
            _lastmessage = new Dictionary<ulong, DateTime>();
        }
    }
}
