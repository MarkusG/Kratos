using System;
using System.ComponentModel.DataAnnotations;

namespace Kratos.EntityFramework
{
    public abstract class RecordBase
    {
        [Key]
        public int Key { get; set; }

        public ulong GuildId { get; set; }

        public ulong ModeratorId { get; set; }

        public ulong SubjectId { get; set; }

        public string Reason { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
