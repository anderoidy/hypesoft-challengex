using Hypesoft.Domain.Repositories;
using Hypesoft.Infrastructure.Data;
using Hypesoft.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hypesoft.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            // ✅ EF Core + MongoDB Provider (com configuração mais robusta)
            var mongoDbSettings = configuration.GetSection("MongoDbSettings");
            var connectionString = mongoDbSettings["ConnectionString"] ?? "mongodb://mongodb:27017/hypesoft_challenge";
            var databaseName = mongoDbSettings["DatabaseName"] ?? "hypesoft_challenge";

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMongoDB(connectionString, databaseName)
            );

            // ✅ Repository Registration (TODOS OS REPOSITÓRIOS)
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            // ✅ Adicionar outros serviços de infraestrutura se necessário
            // services.AddScoped<IEmailService, EmailService>();
            // services.AddScoped<IFileStorageService, FileStorageService>();

            return services;
        }
    }
}
