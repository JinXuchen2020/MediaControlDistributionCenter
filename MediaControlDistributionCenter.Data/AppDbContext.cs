using MediaControlDistributionCenter.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace MediaControlDistributionCenter.Services
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=mydatabase.db");  // SQLite数据库路径
        }
    }
}
