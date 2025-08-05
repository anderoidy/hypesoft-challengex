using Hypesoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Extensions;

namespace Hypesoft.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IUnitOfWork
{
    private readonly IMongoDatabase _mongoDatabase;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        var client = new MongoClient(options.FindExtension<MongoOptionsExtension>()?.ConnectionString);
        _mongoDatabase = client.GetDatabase(options.FindExtension<MongoOptionsExtension>()?.DatabaseName);
    }

    // DbSets para cada entidade
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductTag> ProductTags => Set<ProductTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração do MongoDB para cada entidade
        modelBuilder.Entity<Product>().ToCollection("products");
        modelBuilder.Entity<Category>().ToCollection("categories");
        modelBuilder.Entity<Tag>().ToCollection("tags");
        modelBuilder.Entity<ProductVariant>().ToCollection("product_variants");
        modelBuilder.Entity<ProductTag>().ToCollection("product_tags");

        // Configurações adicionais de índice podem ser adicionadas aqui
        // Exemplo: modelBuilder.Entity<Product>().HasIndex(p => p.SKU).IsUnique();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is EntityBase && (
                e.State == EntityState.Added || 
                e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            var entity = (EntityBase)entityEntry.Entity;
            var now = DateTime.UtcNow;

            if (entityEntry.State == EntityState.Added)
            {
                entity.CreatedAt = now;
                // UpdatedAt também é definido para o mesmo valor inicial
                entity.UpdatedAt = now;
            }
            else if (entityEntry.State == EntityState.Modified)
            {
                entity.UpdatedAt = now;
                // Garante que CreatedAt não seja modificado em atualizações
                Entry(entity).Property(x => x.CreatedAt).IsModified = false;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    // Implementação dos métodos da interface IUnitOfWork
    public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
    {
        return new Repository<TEntity>(this);
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);
        return result > 0;
    }
}
