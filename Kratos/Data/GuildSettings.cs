using System;
using System.Collections.Generic;
using System.Text;

namespace Kratos.Data
{
    public class GuildSettings
    {
        public ulong Id { get; set; }

        public string Prefix { get; set; }

        public ulong ModLogId { get; set; }

        public ulong ServerLogId { get; set; }

        public ulong MuteRoleId { get; set; }
    }
}
