using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Kratos.EntityFramework
{
    public class TagValue
    {
        [Key]
        public int Key { get; set; }

        public string Tag { get; set; } 

        public string Value { get; set; }

        public int TimesInvoked { get; set; }

        public ulong AuthorId { get; set; }

        public ulong GuildId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
