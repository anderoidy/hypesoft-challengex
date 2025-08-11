using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Infrastructure.Persistence.Migrations;

public class MongoDbMigrator
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MongoDbMigrator> _logger;
    private readonly Dictionary<int, IMongoDbMigration> _migrations;
    private const string MigrationCollectionName = "__Migrations";

    public MongoDbMigrator(ApplicationDbContext context, ILogger<MongoDbMigrator> logger)
    {
        _context = context;
        _logger = logger;
        _migrations = new Dictionary<int, IMongoDbMigration>();
        
        // Auto-discover and register all migrations in the current assembly
        var migrationTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IMongoDbMigration).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in migrationTypes)
        {
            if (Activator.CreateInstance(type) is IMongoDbMigration migration)
            {
                _migrations[migration.Version] = migration;
            }
        }
    }

    public async Task MigrateAsync()
    {
        _logger.LogInformation("Starting database migration...");
        
        var currentVersion = await GetCurrentVersionAsync();
        _logger.LogInformation("Current database version: {CurrentVersion}", currentVersion);

        var pendingMigrations = _migrations
            .Where(m => m.Key > currentVersion)
            .OrderBy(m => m.Key)
            .ToList();

        if (!pendingMigrations.Any())
        {
            _logger.LogInformation("Database is up to date.");
            return;
        }

        _logger.LogInformation("Found {Count} pending migrations.", pendingMigrations.Count);

        foreach (var (version, migration) in pendingMigrations)
        {
            try
            {
                _logger.LogInformation("Applying migration {Version}: {Description}", 
                    migration.Version, migration.Description);
                
                await migration.Up(_context);
                await UpdateVersionAsync(version);
                
                _logger.LogInformation("Successfully applied migration {Version}", version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying migration {Version}: {ErrorMessage}", 
                    version, ex.Message);
                throw;
            }
        }
    }

    private async Task<int> GetCurrentVersionAsync()
    {
        try
        {
            // This is a simplified example. In a real implementation, you would query your database
            // to get the current schema version from a special collection or document.
            // For MongoDB, you might use a collection like this:
            // var versionDoc = await _context.Database.GetCollection<BsonDocument>(MigrationCollectionName)
            //     .Find(Builders<BsonDocument>.Filter.Empty)
            //     .SortByDescending(d => d["Version"])
            //     .FirstOrDefaultAsync();
            // return versionDoc?["Version"].AsInt32 ?? 0;
            
            // For now, we'll return 0 to indicate no migrations have been applied
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current database version");
            return 0;
        }
    }

    private async Task UpdateVersionAsync(int version)
    {
        try
        {
            // This is a simplified example. In a real implementation, you would update your database
            // to store the current schema version in a special collection or document.
            // For MongoDB, you might do something like this:
            // var collection = _context.Database.GetCollection<BsonDocument>(MigrationCollectionName);
            // var filter = Builders<BsonDocument>.Filter.Eq("_id", "schema_version");
            // var update = Builders<BsonDocument>.Update
            //     .Set("Version", version)
            //     .Set("UpdatedAt", DateTime.UtcNow);
            // await collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating database version to {Version}", version);
            throw;
        }
    }
}
