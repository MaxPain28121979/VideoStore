using Microsoft.EntityFrameworkCore;
using VideoStore.Models;

namespace VideoStore.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Video> Videos { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
    }
}
