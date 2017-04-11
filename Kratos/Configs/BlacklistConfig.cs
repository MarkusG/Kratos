using System;
using System.Linq;
using System.Collections.Generic;
using Kratos.Services;

namespace Kratos.Configs
{
    public class BlacklistConfig
    {
        public List<ChannelBlacklistConfig> ChannelBlacklists { get; set; } = new List<ChannelBlacklistConfig>();

        public IEnumerable<string> GlobalList { get; set; }

        public TimeSpan GlobalMuteTime { get; set; }

        public bool GlobalEnabled { get; set; }

        public static BlacklistConfig FromService(BlacklistService s)
        {
            var result = new BlacklistConfig();

            foreach (var b in s.ChannelBlacklists)
            {
                result.ChannelBlacklists.Add(new ChannelBlacklistConfig
                {
                    ChannelId = b.Channel.Id,
                    List = b.List.Select(x => x.ToString()),
                    MuteTime = b.MuteTime,
                    Enabled = b.Enabled
                });
            }

            result.GlobalList = s.GlobalBlacklist.List.Select(x => x.ToString());
            result.GlobalMuteTime = s.GlobalBlacklist.MuteTime;
            result.GlobalEnabled = s.GlobalBlacklist.Enabled;

            return result;
        }
    }
}
