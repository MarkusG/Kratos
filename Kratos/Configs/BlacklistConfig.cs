using System;
using System.Collections.Generic;
using System.Text;

namespace Kratos.Configs
{
    public class BlacklistConfig
    {
        public ulong MuteRoleId { get; set; }

        public ulong LogChannelId { get; set; }

        public int MuteTimeInMiliseconds { get; set; }

        public bool Enabled { get; set; }

        public string[] Blacklist { get; set; }

        public ulong[] BypassIds { get; set; }
    }
}
