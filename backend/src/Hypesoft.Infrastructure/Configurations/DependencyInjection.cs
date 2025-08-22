using Ardalis.Specification.EntityFrameworkCore;
using Hypesoft.Domain.Entities;
using Hypesoft.Infrastructure.Configurations;
using Hypesoft.Infrastructure.Data;
using Hypesoft.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hypesoft.Infrastructure.Configurations
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configure MongoDB settings
            services.Configure<MongoDbSettings>(configuration.GetSection("MongoDbSettings"));

            // ✅ Configure Entity Framework with MongoDB
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var mongoSettings = configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
                var connectionString = mongoSettings?.ConnectionString ?? "mongodb://localhost:27017";
                var databaseName = mongoSettings?.DatabaseName ?? "HypesoftDb";
                options.UseMongoDB(connectionString, databaseName);
            });

            // ✅ Configure repositories usando RepositoryBase (Ardalis)
            services.AddScoped<RepositoryBase<ApplicationRole>, RoleRepository>();
            services.AddScoped<RepositoryBase<Category>, CategoryRepository>();
            services.AddScoped<RepositoryBase<Product>, ProductRepository>();

            return services;
        }
    }
}
