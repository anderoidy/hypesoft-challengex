using Hypesoft.Domain.Repositories;
using Hypesoft.Infrastructure.Data;
using Hypesoft.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hypesoft.Infrastructure.Configurations
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            // ✅ EF Core + MongoDB Provider
            var connectionString =
                configuration["MongoDbSettings:ConnectionString"]
                ?? "mongodb://mongodb:27017/hypesoft_challenge";
            var databaseName =
                configuration["MongoDbSettings:DatabaseName"] ?? "hypesoft_challenge";

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMongoDB(connectionString, databaseName)
            );

            // ✅ REGISTRAR INTERFACES CORRETAS:
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }
    }
}
