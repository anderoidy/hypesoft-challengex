using Hypesoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ CONFIGURAR COLLECTIONS PARA MONGODB:
            modelBuilder.Entity<ApplicationUser>().ToCollection("users");
            modelBuilder.Entity<ApplicationRole>().ToCollection("roles");
            modelBuilder.Entity<Product>().ToCollection("products");
            modelBuilder.Entity<Category>().ToCollection("categories");

            // ✅ PRODUCT
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever(); // 👈 Guid controlado pela app
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.CategoryId);

                entity.Ignore(e => e.Categories);
                entity.Ignore(e => e.Category);
            });

            // ✅ CATEGORY
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever(); // 👈 Guid controlado pela app
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.ParentCategoryId);

                entity.Ignore(e => e.ParentCategory);
                entity.Ignore(e => e.ChildCategories);
                entity.Ignore(e => e.SubCategories);
                entity.Ignore(e => e.Products);
            });

            // ✅ APPLICATIONUSER
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever(); // 👈 Guid controlado pela app
                entity.Property(e => e.UserName);
                entity.Property(e => e.Email);

                entity.Ignore(e => e.UserRoles);
            });

            // ✅ APPLICATIONROLE
            modelBuilder.Entity<ApplicationRole>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever(); // 👈 Guid controlado pela app
                entity.Property(e => e.Name);
            });
        }
    }
}
