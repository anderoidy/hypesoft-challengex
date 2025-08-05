using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using System.Reflection;

namespace Hypesoft.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Registra todos os validators do FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        return services;
    }
}