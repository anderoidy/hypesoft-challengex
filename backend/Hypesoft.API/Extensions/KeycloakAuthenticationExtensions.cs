using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Hypesoft.API.Extensions;

public static class KeycloakAuthenticationExtensions
{
    public static IServiceCollection AddKeycloakAuthentication(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var keycloakConfig = configuration.GetSection("Keycloak");
        var authority = keycloakConfig["Authority"];
        var audience = keycloakConfig["Audience"];
        var requireHttpsMetadata = configuration.GetValue<bool>("Keycloak:RequireHttpsMetadata");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority = authority;
            options.Audience = audience;
            options.RequireHttpsMetadata = requireHttpsMetadata;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero
            };
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers.Add("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }

    public static IApplicationBuilder UseKeycloakAuthentication(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        
        return app;
    }
}
