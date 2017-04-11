using System;
using System.Collections.Generic;
using System.Text;

namespace Kratos.Configs
{
    public class ChannelBlacklistConfig
    {
        public ulong ChannelId { get; set; }

        public IEnumerable<string> List { get; set; }

        public TimeSpan MuteTime { get; set; }

        public bool Enabled { get; set; }
    }
}
