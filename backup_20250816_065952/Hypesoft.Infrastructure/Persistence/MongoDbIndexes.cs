using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hypesoft.Domain.Entities;
using MongoDB.Driver;

namespace Hypesoft.Infrastructure.Persistence
{
    public static class MongoDbIndexes
    {
        public static async Task CreateIndexes(IMongoDatabase database)
        {
            await CreateUserIndexes(database);
            await CreateRoleIndexes(database);
            await CreateUserRoleIndexes(database);
            await CreateProductIndexes(database);
            await CreateCategoryIndexes(database);
        }

        private static async Task CreateUserIndexes(IMongoDatabase database)
        {
            var collection = database.GetCollection<ApplicationUser>("users");
            var indexKeys = Builders<ApplicationUser>
                .IndexKeys.Ascending(u => u.Email)
                .Ascending(u => u.NormalizedEmail)
                .Ascending(u => u.UserName)
                .Ascending(u => u.NormalizedUserName);

            var indexOptions = new CreateIndexOptions { Unique = true };
            var model = new CreateIndexModel<ApplicationUser>(indexKeys, indexOptions);
            await collection.Indexes.CreateOneAsync(model);

            // Add text index for search
            var textIndexKeys = Builders<ApplicationUser>
                .IndexKeys.Text(u => u.FirstName)
                .Text(u => u.LastName)
                .Text(u => u.Email);

            var textIndexOptions = new CreateIndexOptions { Name = "TextSearch" };
            var textIndexModel = new CreateIndexModel<ApplicationUser>(
                textIndexKeys,
                textIndexOptions
            );
            await collection.Indexes.CreateOneAsync(textIndexModel);
        }

        private static async Task CreateRoleIndexes(IMongoDatabase database)
        {
            var collection = database.GetCollection<ApplicationRole>("roles");
            var indexKeys = Builders<ApplicationRole>.IndexKeys.Ascending(r => r.NormalizedName);

            var indexOptions = new CreateIndexOptions { Unique = true };
            var model = new CreateIndexModel<ApplicationRole>(indexKeys, indexOptions);
            await collection.Indexes.CreateOneAsync(model);
        }

        private static async Task CreateUserRoleIndexes(IMongoDatabase database)
        {
            var collection = database.GetCollection<ApplicationUserRole>("user_roles");

            // Compound index for user-role relationship
            var userRoleKeys = Builders<ApplicationUserRole>
                .IndexKeys.Ascending(ur => ur.UserId)
                .Ascending(ur => ur.RoleId);

            var userRoleOptions = new CreateIndexOptions { Unique = true };
            var userRoleModel = new CreateIndexModel<ApplicationUserRole>(
                userRoleKeys,
                userRoleOptions
            );
            await collection.Indexes.CreateOneAsync(userRoleModel);

            // Single field indexes for faster lookups
            var userIdIndex = Builders<ApplicationUserRole>.IndexKeys.Ascending(ur => ur.UserId);
            await collection.Indexes.CreateOneAsync(
                new CreateIndexModel<ApplicationUserRole>(userIdIndex)
            );

            var roleIdIndex = Builders<ApplicationUserRole>.IndexKeys.Ascending(ur => ur.RoleId);
            await collection.Indexes.CreateOneAsync(
                new CreateIndexModel<ApplicationUserRole>(roleIdIndex)
            );
        }

        private static async Task CreateProductIndexes(IMongoDatabase database)
        {
            var collection = database.GetCollection<Product>("products");

            // Text index for product search
            var textIndexKeys = Builders<Product>
                .IndexKeys.Text(p => p.Name)
                .Text(p => p.Description);

            var textIndexOptions = new CreateIndexOptions { Name = "TextSearch" };
            var textIndexModel = new CreateIndexModel<Product>(textIndexKeys, textIndexOptions);
            await collection.Indexes.CreateOneAsync(textIndexModel);

            // Index for category and stock status
            var categoryIndex = Builders<Product>.IndexKeys.Ascending(p => p.CategoryId);
            await collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>(categoryIndex));

            var stockIndex = Builders<Product>.IndexKeys.Ascending(p => p.StockQuantity);
            await collection.Indexes.CreateOneAsync(new CreateIndexModel<Product>(stockIndex));
        }

        private static async Task CreateCategoryIndexes(IMongoDatabase database)
        {
            var collection = database.GetCollection<Category>("categories");

            // Text index for category search
            var textIndexKeys = Builders<Category>.IndexKeys.Text(c => c.Name);

            var textIndexOptions = new CreateIndexOptions { Name = "TextSearch" };
            var textIndexModel = new CreateIndexModel<Category>(textIndexKeys, textIndexOptions);
            await collection.Indexes.CreateOneAsync(textIndexModel);

            // Index for parent category relationship
            var parentCategoryIndex = Builders<Category>.IndexKeys.Ascending(c =>
                c.ParentCategoryId
            );
            await collection.Indexes.CreateOneAsync(
                new CreateIndexModel<Category>(parentCategoryIndex)
            );
        }
    }
}
