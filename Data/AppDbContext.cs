using Microsoft.EntityFrameworkCore;
using XownerWebOne.Models;

namespace XownerWebOne.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Seller> Sellers { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<User> Users { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Price)
                      .HasPrecision(18, 2);

                entity.Property(p => p.OriginalPrice)
                      .HasPrecision(18, 2);

                entity.OwnsOne(p => p.Specification);
            });

            base.OnModelCreating(modelBuilder);
        }

    }
}
