using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Hypesoft.Application.Common.Interfaces;
using Hypesoft.Infrastructure.Persistence;
using Hypesoft.Domain.Repositories;
using Hypesoft.Infrastructure.Repositories;
using Hypesoft.Domain.Entities; 

namespace Hypesoft.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var databaseName = configuration["MongoDB:DatabaseName"];
        
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMongoDB(connectionString, databaseName));
            
        // Outros serviços...
        
        return services;
    }
}