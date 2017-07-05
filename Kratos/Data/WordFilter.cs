using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Kratos.Data
{
    public class WordFilter
    {
        public ulong Id { get; set; }

        public List<Regex> Patterns { get; set; } = new List<Regex>();

        public TimeSpan MuteTime { get; set; } = TimeSpan.FromMinutes(15);

        public bool Enabled { get; set; } = true;

        public WordFilterType Type { get; set; }

        public WordFilter(ulong id, WordFilterType type)
        {
            Id = id;
            Type = type;
        }
    }
}
