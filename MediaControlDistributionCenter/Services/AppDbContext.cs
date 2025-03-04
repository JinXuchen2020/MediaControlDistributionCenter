using MediaControlDistributionCenter.Models;

using Microsoft.EntityFrameworkCore;

namespace MediaControlDistributionCenter.Services
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=mydatabase.db");  // SQLite数据库路径
        }
    }
}
