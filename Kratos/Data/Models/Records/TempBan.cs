using System;

namespace Kratos.Data
{
    public class TempBan : ModeratorAction
    {
        public bool Active { get; set; }

        public DateTime UnbanAt { get; set; }

        public string SubjectName { get; set; }
    }
}
