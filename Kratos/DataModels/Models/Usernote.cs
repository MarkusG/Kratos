using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Kratos.Data
{
    public class Usernote
    {
        [Key]
        public int Key { get; set; }

        public ulong SubjectId { get; set; }

        public ulong AuthorId { get; set; }
        // This will no longer work after 2/07/2106 6:28 AM UTC
        public uint UnixTimestamp { get; set; }
        
        public string Content { get; set; }
    }
}
