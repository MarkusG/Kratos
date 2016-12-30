﻿using System;
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
        
        public ulong UnixTimestamp { get; set; }
        
        public string Content { get; set; }
    }
}
