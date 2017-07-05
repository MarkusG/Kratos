using Microsoft.EntityFrameworkCore;

namespace Kratos.EntityFramework
{
    public class KratosContext : DbContext
    {
        public DbSet<PermissionPair> Permissions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            base.OnConfiguring(optionsBuilder.UseSqlite($"Data Source={Program.GetOriginalDirectory() + "Kratos.sqlite"};"));
    }
}
