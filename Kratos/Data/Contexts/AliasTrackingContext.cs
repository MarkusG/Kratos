using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Kratos.Data
{
    class AliasTrackingContext : DbContext
    {
        public DbSet<UsernameAlias> UsernameAliases { get; set; }

        public DbSet<NicknameAlias> NicknameAliases { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            base.OnConfiguring(optionsBuilder.UseSqlite("Data Source=UserAliases.sqlite;"));
    }
}
