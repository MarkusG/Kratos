using Microsoft.EntityFrameworkCore;
using Kratos.Data.Models;

namespace Kratos.Data.Contexts
{
    class TagContext : DbContext
    {
        public DbSet<TagValue> Tags { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=Tags.sqlite;");
        }
    }
}
