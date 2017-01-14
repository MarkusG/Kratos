using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Kratos.Data
{
    class UsernoteContext : DbContext
    {
        public DbSet<Usernote> Notes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=Usernotes.sqlite;");
        }
    }
}
