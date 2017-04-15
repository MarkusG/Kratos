using System;
using System.ComponentModel.DataAnnotations;

namespace Kratos.Data
{
    public class NicknameAlias
    {
        [Key]
        public int Key { get; set; }

        public ulong UserId { get; set; }

        public ulong GuildId { get; set; }

        public DateTime Until { get; set; }

        public string Alias { get; set; }
    }
}
