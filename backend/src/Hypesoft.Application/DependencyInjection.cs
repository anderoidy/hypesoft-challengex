using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MediatR;
using AutoMapper;
using System.Reflection;
using Hypesoft.Domain.Common.Interfaces;
using System.Linq.Expressions;


namespace Hypesoft.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {

        // Registra todos os validators do FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // MediatR handlers
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // AutoMapper profiles
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        
        return services;
    }
}