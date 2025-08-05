using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Hypesoft.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Configuração do MongoDB (quando necessário)
        // var connectionString = configuration.GetConnectionString("MongoDB");
        // services.AddDbContext<YourDbContext>(options =>
        //     options.UseMongoDB(connectionString, "YourDatabaseName"));
        
        // Registre seus repositórios aqui
        // services.AddScoped<IProductRepository, ProductRepository>();
        
        return services;
    }
}