using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using KriptoProyek.Models;

namespace KriptoProyek.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Contoh konfigurasi tambahan, tanpa HasData
            builder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Name).IsRequired();
                entity.Property(p => p.Description).IsRequired();
                entity.Property(p => p.Price).HasColumnType("numeric");
            });
        }
    }
}
