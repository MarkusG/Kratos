using System;
using System.Collections.Generic;
using System.Text;

namespace Kratos.Configs
{
    public class BlacklistConfig
    {
        public int MuteTimeInSeconds { get; set; }

        public bool Enabled { get; set; }

        public string[] Blacklist { get; set; }
    }
}
