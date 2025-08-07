using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Hypesoft.Application.Common.Interfaces;
using Hypesoft.Infrastructure.Persistence;
using Hypesoft.Domain.Repositories;
using Hypesoft.Infrastructure.Repositories;

namespace Hypesoft.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // DbContext (MongoDB EF Core)
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMongoDB(
                configuration["Mongo:ConnectionString"],
                configuration["Mongo:Database"]));

        // Repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        // TODO: TagRepository implementation placeholder - register when created
        services.AddScoped<ITagRepository, RepositoryBase<Tag>>();

        // Unit of Work
        services.AddScoped<IApplicationUnitOfWork, ApplicationUnitOfWork>();

        return services;
    }
}