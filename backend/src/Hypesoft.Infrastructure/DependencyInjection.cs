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
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // DbContext (MongoDB EF Core)
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMongoDB(
                configuration["Mongo:ConnectionString"] ?? throw new ArgumentNullException("Mongo:ConnectionString not configured"),
                configuration["Mongo:Database"] ?? throw new ArgumentNullException("Mongo:Database not configured")));

        // Repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITagRepository, TagRepository>(); 

        // Unit of Work
        services.AddScoped<IApplicationUnitOfWork, ApplicationUnitOfWork>();

        return services;
    }
}