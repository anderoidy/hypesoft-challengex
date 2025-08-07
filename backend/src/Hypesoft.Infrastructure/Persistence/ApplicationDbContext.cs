using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;
using Hypesoft.Domain.Entities;

namespace Hypesoft.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Mapeamentos e índices MongoDB
        modelBuilder.Entity<Product>().HasKey(p => p.Id);
        modelBuilder.Entity<Product>().HasIndex(p => p.Sku).IsUnique();
        modelBuilder.Entity<Product>().HasIndex(p => p.Barcode).IsUnique();
        modelBuilder.Entity<Category>().HasKey(c => c.Id);
        modelBuilder.Entity<Tag>().HasKey(t => t.Id);

        base.OnModelCreating(modelBuilder);
    }
}
