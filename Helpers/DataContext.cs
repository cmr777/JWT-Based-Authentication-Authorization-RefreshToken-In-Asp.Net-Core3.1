using Microsoft.EntityFrameworkCore;
using WebApi.Entities;

namespace WebApi.Helpers
{
    public class DataContext : DbContext
    {
        public DbSet<UserMaster> UserMaster { get; set; }
        public DbSet<RefreshToken> RefreshToken { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
    }
}