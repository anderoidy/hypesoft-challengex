using System;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Entities;
using Hypesoft.Infrastructure.Configurations;
using Hypesoft.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hypesoft.Infrastructure.Configurations
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            // Configure MongoDB settings
            services.Configure<MongoDbSettings>(configuration.GetSection("MongoDbSettings"));

            // Register MongoDB client and database as singleton
            services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
                return new MongoClient(settings?.ConnectionString);
            });

            services.AddScoped<IMongoDatabase>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                var settings = configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
                return client.GetDatabase(settings?.DatabaseName);
            });

            // Register ApplicationDbContext
            services.AddScoped<ApplicationDbContext>(sp =>
            {
                var database = sp.GetRequiredService<IMongoDatabase>();
                return new ApplicationDbContext(database);
            });

            // Register Unit of Work
            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

            // Register repositories
            services.AddScoped<IRepository<ApplicationUser>, RepositoryBase<ApplicationUser>>();
            services.AddScoped<IRepository<ApplicationRole>, RepositoryBase<ApplicationRole>>();
            services.AddScoped<
                IRepository<ApplicationUserRole>,
                RepositoryBase<ApplicationUserRole>
            >();
            services.AddScoped<IRepository<Product>, RepositoryBase<Product>>();
            services.AddScoped<IRepository<Category>, RepositoryBase<Category>>();

            // Register custom repositories
            services.AddScoped<IUserRepository, UserRepository>();
            // Add other custom repositories here

            return services;
        }
    }
}
