using Hypesoft.Domain.Entities;
using MongoDB.Driver;

namespace Hypesoft.Infrastructure.Persistence.Configurations;

public static class MongoIndexConfigurations
{
    public static void ConfigureIndexes(IMongoDatabase database)
    {
        // Índices para a coleção de Produtos
        var productsCollection = database.GetCollection<Product>("products");
        
        // Índice para busca por SKU (único)
        var skuIndexModel = new CreateIndexModel<Product>(
            Builders<Product>.IndexKeys.Ascending(p => p.Sku),
            new CreateIndexOptions { Unique = true, Sparse = true });
            
        // Índice para busca por código de barras (único)
        var barcodeIndexModel = new CreateIndexModel<Product>(
            Builders<Product>.IndexKeys.Ascending(p => p.Barcode),
            new CreateIndexOptions { Unique = true, Sparse = true });
            
        // Índice para busca por categoria e status de publicação
        var categoryPublishedIndexModel = new CreateIndexModel<Product>(
            Builders<Product>.IndexKeys
                .Ascending(p => p.CategoryId)
                .Ascending(p => p.IsPublished)
                .Ascending(p => p.IsFeatured));
                
        // Aplica os índices
        productsCollection.Indexes.CreateMany(new[] 
        { 
            skuIndexModel, 
            barcodeIndexModel, 
            categoryPublishedIndexModel 
        });

        // Índices para a coleção de Categorias
        var categoriesCollection = database.GetCollection<Category>("categories");
        
        // Índice composto para categoria pai e nome (para garantir unicidade)
        var categoryNameIndexModel = new CreateIndexModel<Category>(
            Builders<Category>.IndexKeys
                .Ascending(c => c.ParentCategoryId)
                .Ascending(c => c.Name),
            new CreateIndexOptions { Unique = true });
            
        // Índice para busca por slug
        var categorySlugIndexModel = new CreateIndexModel<Category>(
            Builders<Category>.IndexKeys.Ascending(c => c.Slug),
            new CreateIndexOptions { Unique = true });
            
        // Aplica os índices
        categoriesCollection.Indexes.CreateMany(new[] 
        { 
            categoryNameIndexModel, 
            categorySlugIndexModel 
        });

        // Índices para a coleção de Tags
        var tagsCollection = database.GetCollection<Tag>("tags");
        
        // Índice para nome da tag (único)
        var tagNameIndexModel = new CreateIndexModel<Tag>(
            Builders<Tag>.IndexKeys.Ascending(t => t.Name),
            new CreateIndexOptions { Unique = true });
            
        // Aplica os índices
        tagsCollection.Indexes.CreateOne(tagNameIndexModel);
        
        // Índices para a coleção de ProductTags (relação muitos-para-muitos)
        var productTagsCollection = database.GetCollection<ProductTag>("product_tags");
        
        // Índice composto para produto e tag (garante unicidade)
        var productTagIndexModel = new CreateIndexModel<ProductTag>(
            Builders<ProductTag>.IndexKeys
                .Ascending(pt => pt.ProductId)
                .Ascending(pt => pt.TagId),
            new CreateIndexOptions { Unique = true });
            
        // Índice para busca por tag
        var tagIdIndexModel = new CreateIndexModel<ProductTag>(
            Builders<ProductTag>.IndexKeys.Ascending(pt => pt.TagId));
            
        // Aplica os índices
        productTagsCollection.Indexes.CreateMany(new[] 
        { 
            productTagIndexModel, 
            tagIdIndexModel 
        });
        
        // Índices para a coleção de ProductVariants
        var variantsCollection = database.GetCollection<ProductVariant>("product_variants");
        
        // Índice para busca por produto
        var productIdIndexModel = new CreateIndexModel<ProductVariant>(
            Builders<ProductVariant>.IndexKeys.Ascending(pv => pv.ProductId));
            
        // Índice para SKU (único)
        var variantSkuIndexModel = new CreateIndexModel<ProductVariant>(
            Builders<ProductVariant>.IndexKeys.Ascending(pv => pv.Sku),
            new CreateIndexOptions { Unique = true, Sparse = true });
            
        // Índice para código de barras (único)
        var variantBarcodeIndexModel = new CreateIndexModel<ProductVariant>(
            Builders<ProductVariant>.IndexKeys.Ascending(pv => pv.Barcode),
            new CreateIndexOptions { Unique = true, Sparse = true });
            
        // Aplica os índices
        variantsCollection.Indexes.CreateMany(new[] 
        { 
            productIdIndexModel, 
            variantSkuIndexModel, 
            variantBarcodeIndexModel 
        });
    }
}
