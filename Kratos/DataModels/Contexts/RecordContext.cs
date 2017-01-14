using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Kratos.Data
{
    public class RecordContext : DbContext
    {
        public DbSet<PermaBan> PermaBans { get; set; }

        public DbSet<TempBan> TempBans { get; set; }

        public DbSet<SoftBan> SoftBans { get; set; }

        public DbSet<Mute> Mutes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=Records.sqlite;");
        }
    }
}
