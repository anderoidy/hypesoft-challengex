using Hypesoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Extensions;

namespace Hypesoft.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<ApplicationRole> Roles { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseMongoDB("mongodb://localhost:27017", "HypesoftDb")
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors()
                    .LogTo(Console.WriteLine, LogLevel.Information);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ CONFIGURAR COLLECTIONS PARA MONGODB:
            modelBuilder.Entity<ApplicationUser>().ToCollection("users");
            modelBuilder.Entity<ApplicationRole>().ToCollection("roles");
            modelBuilder.Entity<Product>().ToCollection("products");
            modelBuilder.Entity<Category>().ToCollection("categories");

            // ✅ PRODUCT - IGNORAR TODAS AS NAVEGAÇÕES:
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.CategoryId);

                // ✅ IGNORAR NAVEGAÇÕES PROBLEMÁTICAS:
                entity.Ignore(e => e.Categories);
                entity.Ignore(e => e.Category);
            });

            // ✅ CATEGORY - IGNORAR TODAS AS NAVEGAÇÕES:
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.ParentCategoryId);

                // ✅ IGNORAR NAVEGAÇÕES PROBLEMÁTICAS:
                entity.Ignore(e => e.ParentCategory);
                entity.Ignore(e => e.ChildCategories);
                entity.Ignore(e => e.SubCategories);
                entity.Ignore(e => e.Products);
            });

            // ✅ APPLICATIONUSER - BÁSICO:
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserName);
                entity.Property(e => e.Email);

                // ✅ IGNORAR NAVEGAÇÕES:
                entity.Ignore(e => e.UserRoles);
            });

            // ✅ APPLICATIONROLE - BÁSICO:
            modelBuilder.Entity<ApplicationRole>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name);
            });
        }
    }
}
