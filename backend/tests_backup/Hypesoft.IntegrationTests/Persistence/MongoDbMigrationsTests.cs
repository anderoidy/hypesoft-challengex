using System.Threading.Tasks;
using Hypesoft.Infrastructure.Persistence;
using Hypesoft.Infrastructure.Persistence.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Hypesoft.IntegrationTests.Persistence;

public class MongoDbMigrationsTests : IClassFixture<TestBase>
{
    private readonly TestBase _testBase;
    private readonly ApplicationDbContext _dbContext;
    private readonly MongoDbMigrator _migrator;

    public MongoDbMigrationsTests(TestBase testBase)
    {
        _testBase = testBase;
        _dbContext = _testBase.DbContext;

        // Get logger from service provider
        var loggerFactory = _testBase.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<MongoDbMigrator>();

        _migrator = new MongoDbMigrator(_dbContext, logger);
    }

    [Fact]
    public async Task MigrateAsync_ShouldCreateRequiredIndexes()
    {
        // Arrange - Get the products collection
        var productsCollection = _dbContext.Products;

        // Act - Run migrations
        await _migrator.MigrateAsync();

        // Assert - Verify indexes were created
        using var cursor = await productsCollection
            .Database.GetCollection<object>("Products")
            .Indexes.ListAsync();

        var indexes = await cursor.ToListAsync();

        // Check for Sku index
        Assert.Contains(
            indexes,
            i => i["name"].AsString == "sku_1" && i["unique"].AsBoolean == true
        );

        // Check for Barcode index
        Assert.Contains(
            indexes,
            i => i["name"].AsString == "barcode_1" && i["unique"].AsBoolean == true
        );

        // Check for text index on name and description
        Assert.Contains(
            indexes,
            i => i["name"].AsString == "name_text_description_text" && i["textIndexVersion"] != null
        );
    }

    [Fact]
    public async Task MigrateAsync_ShouldBeIdempotent()
    {
        // Arrange - Run migrations once
        await _migrator.MigrateAsync();

        // Act - Run migrations again
        var exception = await Record.ExceptionAsync(() => _migrator.MigrateAsync());

        // Assert - No exception should be thrown on second run
        Assert.Null(exception);
    }
}
