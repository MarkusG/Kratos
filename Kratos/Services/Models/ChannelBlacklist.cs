using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Discord.WebSocket;

namespace Kratos.Services.Models
{
    public class ChannelBlacklist
    {
        public SocketTextChannel Channel { get; set; }

        public List<Regex> List { get; set; }

        public TimeSpan MuteTime { get; set; }

        public bool Enabled { get; set; }

        public bool CheckViolation(string message) =>
            Enabled && List.Any(x => x.IsMatch(message));

        public ChannelBlacklist()
        {

        }

        public ChannelBlacklist(SocketTextChannel channel, TimeSpan muteTime)
        {
            Channel = channel;
            List = new List<Regex>();
            MuteTime = muteTime;
            Enabled = true;
        }
    }
}
