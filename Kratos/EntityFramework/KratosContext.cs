using System;
using Microsoft.EntityFrameworkCore;

namespace Kratos.EntityFramework
{
    public class KratosContext : DbContext
    {
        public DbSet<PermissionPair> Permissions { get; set; }

        public DbSet<BanRecord> BanRecords { get; set; }

        public DbSet<MuteRecord> MuteRecords { get; set; }

        public DbSet<SoftBanRecord> SoftBanRecords { get; set; }

        public DbSet<TemporaryBanRecord> TemporaryBanRecords { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            base.OnConfiguring(optionsBuilder.UseSqlite($"Data Source={Program.GetOriginalDirectory() + "Kratos.sqlite"};"));
    }
}
