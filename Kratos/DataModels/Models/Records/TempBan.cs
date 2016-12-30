using System;
using System.Collections.Generic;
using System.Text;

namespace Kratos.Data
{
    public class TempBan : ModeratorAction
    {
        public bool Active { get; set; }

        public ulong UnbanAtUnixTimestamp { get; set; }

        public string SubjectName { get; set; }
    }
}
