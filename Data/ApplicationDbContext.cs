using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using KriptoProyek.Models;

namespace KriptoProyek.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<UserToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Konfigurasi roles dengan GUID STATIS (tidak berubah setiap build)
        builder.Entity<IdentityRole>().HasData(
            new IdentityRole 
            { 
                Id = "a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d", 
                Name = "Admin", 
                NormalizedName = "ADMIN",
                ConcurrencyStamp = "a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d" // STATIS!
            },
            new IdentityRole 
            { 
                Id = "b2c3d4e5-f6a7-5b6c-9d0e-1f2a3b4c5d6e", 
                Name = "User", 
                NormalizedName = "USER",
                ConcurrencyStamp = "b2c3d4e5-f6a7-5b6c-9d0e-1f2a3b4c5d6e" // STATIS!
            }
        );

        // Konfigurasi UserToken
        builder.Entity<UserToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.ExpiresAt).IsRequired();
            entity.Property(e => e.IsRevoked).IsRequired().HasDefaultValue(false);
            
            // Index untuk performa query
            entity.HasIndex(e => e.Token);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.IsRevoked);
            
            // Relasi dengan User
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}