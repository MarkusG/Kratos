using System.Collections.Generic;

namespace Kratos.Configs
{
    public class BlacklistConfig
    {
        public int MuteTimeInSeconds { get; set; }

        public bool Enabled { get; set; }

        public IEnumerable<string> Blacklist { get; set; }
    }
}
