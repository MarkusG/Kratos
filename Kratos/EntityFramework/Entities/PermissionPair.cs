using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Kratos.EntityFramework
{
    public class PermissionPair
    {
        [Key]
        public ulong Id { get; set; }

        public string Permissions { get; set; }
    }
}
