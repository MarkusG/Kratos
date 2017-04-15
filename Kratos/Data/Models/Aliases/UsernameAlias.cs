using System;   
using System.ComponentModel.DataAnnotations;

namespace Kratos.Data
{
    public class UsernameAlias
    {
        [Key]
        public int Key { get; set; }

        public ulong UserId { get; set; }

        public DateTime Until { get; set; }

        public string Alias { get; set; }
    }
}
