using EvergreenRanch.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EvergreenRanch.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Animal> Animals { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> Items { get; set; }
    }
}
