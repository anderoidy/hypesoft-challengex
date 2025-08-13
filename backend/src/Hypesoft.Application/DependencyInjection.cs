using System.Linq.Expressions;
using System.Reflection;
using AutoMapper;
using FluentValidation;
using Hypesoft.Domain.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Hypesoft.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Registra todos os validators do FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // MediatR handlers
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
        );

        // AutoMapper profiles
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        return services;
    }
}
