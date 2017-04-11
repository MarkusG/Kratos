using System;
using System.Collections.Generic;
using System.Text;
using Kratos.Services.Models;

namespace Kratos.Services.Results
{
    public class BlacklistViolationResult
    {
        public bool IsViolating { get; set; }

        public ChannelBlacklist Blacklist { get; set; }
    }
}
