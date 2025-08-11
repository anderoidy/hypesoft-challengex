using System.Threading.Tasks;
using Hypesoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Infrastructure.Persistence.Migrations;

public class InitialProductMigration : IMongoDbMigration
{
    public int Version => 1;
    public string Description => "Initial migration for Products collection";

    public async Task Up(ApplicationDbContext context)
    {
        // Create indexes for Products collection
        var products = context.Database.GetCollection<Product>("Products");
        
        // Create a unique index on Sku
        var skuIndexModel = new Microsoft.EntityFrameworkCore.MongoDB.IndexKeysDefinitionBuilder<Product>()
            .Ascending(p => p.Sku);
        
        await products.Indexes.CreateOneAsync(new Microsoft.EntityFrameworkCore.MongoDB.IndexKeysDefinitionBuilder<Product>()
            .Ascending(p => p.Sku), new Microsoft.EntityFrameworkCore.MongoDB.CreateIndexOptions { Unique = true });
        
        // Create an index on Barcode
        await products.Indexes.CreateOneAsync(new Microsoft.EntityFrameworkCore.MongoDB.IndexKeysDefinitionBuilder<Product>()
            .Ascending(p => p.Barcode), new Microsoft.EntityFrameworkCore.MongoDB.CreateIndexOptions { Unique = true });
        
        // Create a text index for search
        await products.Indexes.CreateOneAsync(new Microsoft.EntityFrameworkCore.MongoDB.IndexKeysDefinitionBuilder<Product>()
            .Text(p => p.Name)
            .Text(p => p.Description));
    }

    public Task Down(ApplicationDbContext context)
    {
        // In a real implementation, you would drop the indexes here
        // For now, we'll just complete the task
        return Task.CompletedTask;
    }
}
