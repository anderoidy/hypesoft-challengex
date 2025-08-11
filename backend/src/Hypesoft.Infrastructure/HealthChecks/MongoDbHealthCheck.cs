using System;
using System.Threading;
using System.Threading.Tasks;
using Hypesoft.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Hypesoft.Infrastructure.HealthChecks
{
    public class MongoDbHealthCheck : IHealthCheck
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<MongoDbHealthCheck> _logger;

        public MongoDbHealthCheck(ApplicationDbContext dbContext, ILogger<MongoDbHealthCheck> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Checking MongoDB health...");
                
                // Check if we can connect to the database
                var database = _dbContext.Database;
                var pingCommand = new { ping = 1 };
                
                await database.RunCommandAsync(
                    (Command<MongoDB.Bson.BsonDocument>)pingCommand, 
                    cancellationToken: cancellationToken);
                
                // Check if we can query a collection
                var collection = _dbContext.Products;
                var count = await collection.CountDocumentsAsync(
                    Builders<Hypesoft.Domain.Entities.Product>.Filter.Empty, 
                    cancellationToken: cancellationToken);
                
                _logger.LogInformation("MongoDB health check successful");
                
                return HealthCheckResult.Healthy(
                    "MongoDB is healthy and responding to queries.",
                    new Dictionary<string, object>
                    {
                        { "database", database.DatabaseNamespace.DatabaseName },
                        { "total_products", count },
                        { "server_status", "online" },
                        { "server_time", DateTime.UtcNow }
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MongoDB health check failed");
                
                return HealthCheckResult.Unhealthy(
                    "MongoDB is unhealthy",
                    exception: ex,
                    data: new Dictionary<string, object>
                    {
                        { "error", ex.Message },
                        { "stack_trace", ex.StackTrace ?? "No stack trace available" },
                        { "server_time", DateTime.UtcNow }
                    });
            }
        }
    }
}
