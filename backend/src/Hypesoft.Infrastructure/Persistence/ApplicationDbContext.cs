using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Infrastructure.Persistence
{
    /// <summary>
    /// Represents the database context for the application.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IUnitOfWork
    {
        private readonly ILogger<ApplicationDbContext> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
        /// </summary>
        /// <param name="options">The options for this context.</param>
        /// <param name="logger">The logger instance.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ILogger<ApplicationDbContext> logger)
            : base(options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets or sets the database set for products.
        /// </summary>
        public DbSet<Product> Products { get; set; } = null!;

        /// <summary>
        /// Gets or sets the database set for categories.
        /// </summary>
        public DbSet<Category> Categories { get; set; } = null!;

        /// <summary>
        /// Gets or sets the database set for tags.
        /// </summary>
        public DbSet<Tag> Tags { get; set; } = null!;

        /// <summary>
        /// Gets or sets the database set for product tags.
        /// </summary>
        public DbSet<ProductTag> ProductTags { get; set; } = null!;

        /// <inheritdoc />
        public DatabaseFacade DatabaseFacade => Database;

        /// <summary>
        /// Configures the schema needed for the identity framework.
        /// </summary>
        /// <param name="builder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Apply all configurations defined in the current assembly
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Configure the application user entity
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("Users");
                entity.Property(u => u.Id).ValueGeneratedOnAdd();
                entity.Property(u => u.UserName).HasMaxLength(256);
                entity.Property(u => u.NormalizedUserName).HasMaxLength(256);
                entity.Property(u => u.Email).HasMaxLength(256);
                entity.Property(u => u.NormalizedEmail).HasMaxLength(256);
            });

            // Configure the application role entity
            builder.Entity<ApplicationRole>(entity =>
            {
                entity.ToTable("Roles");
                entity.Property(r => r.Id).ValueGeneratedOnAdd();
                entity.Property(r => r.Name).HasMaxLength(256);
                entity.Property(r => r.NormalizedName).HasMaxLength(256);
            });

            // Configure the identity user role entity
            builder.Entity<IdentityUserRole<Guid>>(entity =>
            {
                entity.ToTable("UserRoles");
            });

            // Configure the identity user claim entity
            builder.Entity<IdentityUserClaim<Guid>>(entity =>
            {
                entity.ToTable("UserClaims");
            });

            // Configure the identity user login entity
            builder.Entity<IdentityUserLogin<Guid>>(entity =>
            {
                entity.ToTable("UserLogins");
            });

            // Configure the identity role claim entity
            builder.Entity<IdentityRoleClaim<Guid>>(entity =>
            {
                entity.ToTable("RoleClaims");
            });

            // Configure the identity user token entity
            builder.Entity<IdentityUserToken<Guid>>(entity =>
            {
                entity.ToTable("UserTokens");
            });

            // Configure soft delete query filter for all entities that implement ISoftDelete
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    entityType.AddSoftDeleteQueryFilter();
                }
            }

            // Mapeamentos e índices MongoDB
            builder.Entity<Product>().HasKey(p => p.Id);
            builder.Entity<Product>().HasIndex(p => p.Sku).IsUnique();
            builder.Entity<Product>().HasIndex(p => p.Barcode).IsUnique();
            builder.Entity<Category>().HasKey(c => c.Id);
            builder.Entity<Tag>().HasKey(t => t.Id);
            
            // Configuração do relacionamento muitos-para-muitos entre Product e Tag
            builder.Entity<ProductTag>()
                .HasKey(pt => pt.Id);
                
            builder.Entity<ProductTag>()
                .HasOne(pt => pt.Product)
                .WithMany(p => p.ProductTags)
                .HasForeignKey(pt => pt.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.Entity<ProductTag>()
                .HasOne(pt => pt.Tag)
                .WithMany(t => t.ProductTags)
                .HasForeignKey(pt => pt.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        /// <summary>
        /// Saves all changes made in this context to the database.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity && (
                    e.State == EntityState.Added
                    || e.State == EntityState.Modified
                    || e.State == EntityState.Deleted));

            foreach (var entityEntry in entries)
            {
                var entity = (BaseEntity)entityEntry.Entity;
                var now = DateTimeOffset.UtcNow;

                switch (entityEntry.State)
                {
                    case EntityState.Added:
                        entity.CreatedAt = now;
                        entity.ModifiedAt = now;
                        break;

                    case EntityState.Modified:
                        entity.ModifiedAt = now;
                        break;

                    case EntityState.Deleted:
                        if (entity is ISoftDelete softDeleteEntity)
                        {
                            entityEntry.State = EntityState.Modified;
                            softDeleteEntity.IsDeleted = true;
                            softDeleteEntity.DeletedAt = now;
                        }
                        break;
                }
            }

            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "A concurrency error occurred while saving changes to the database.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving changes to the database.");
                throw;
            }
        }

        /// <summary>
        /// Releases the allocated resources for this context.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
        }

        /// <summary>
        /// Releases the allocated resources for this context.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
        }
    }
}
