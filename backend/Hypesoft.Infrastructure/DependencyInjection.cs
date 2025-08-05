using Hypesoft.Application.Common.Interfaces;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Infrastructure.Persistence;
using Hypesoft.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hypesoft.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Registrar o DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMongoDB(
                configuration.GetConnectionString("DefaultConnection") ?? 
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found."),
                configuration["DatabaseName"] ?? 
                throw new InvalidOperationException("DatabaseName configuration not found.")
            ));

        // Registrar o Unit of Work
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationUnitOfWork>());
        services.AddScoped<IApplicationUnitOfWork, ApplicationUnitOfWork>();

        // Registrar os repositórios
        services.AddScoped<IRepository<Product>, ProductRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        
        services.AddScoped<IRepository<Category>, CategoryRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        
        services.AddScoped<IRepository<Tag>, TagRepository>();
        services.AddScoped<ITagRepository, TagRepository>();

        // Outros serviços de infraestrutura podem ser registrados aqui
        
        return services;
    }
}
