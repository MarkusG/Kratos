using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace Kratos.Services.Models
{
    public class GlobalBlacklist
    {
        public List<Regex> List { get; set; }

        public TimeSpan MuteTime { get; set; }

        public bool Enabled { get; set; } = true;

        public bool CheckViolation(string message) =>
            Enabled && List.Any(x => x.IsMatch(message));
    }
}
