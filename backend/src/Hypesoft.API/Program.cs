using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hypesoft.Application;
using Hypesoft.Application.Commands.Categories;
using Hypesoft.Application.Commands.Products;
using Hypesoft.Application.Handlers.Categories;
using Hypesoft.Application.Handlers.Products;
using Hypesoft.Application.Mapping;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Hypesoft.Infrastructure.Configurations;
using Hypesoft.Infrastructure.Data;
using Hypesoft.Infrastructure.Extensions;
using Hypesoft.Infrastructure.Middleware;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

Console.WriteLine("🚀 [1] Starting application...");

// Criar o builder
var builder = WebApplication.CreateBuilder(args);
Console.WriteLine("✅ [2] Builder created successfully!");

// Configuração
builder
    .Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

Console.WriteLine("📋 [3] Configuration loaded!");

// Configure Serilog
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Host.UseSerilog();
Console.WriteLine("📄 [4] Serilog configured!");

// ===========================================
// ✅ MEDIATR - CONFIGURAÇÃO ÚNICA E LIMPA
// ===========================================
builder.Services.AddMediatR(cfg =>
{
    // Registra TODOS os handlers de TODAS as assemblies necessárias
    cfg.RegisterServicesFromAssemblies(
        typeof(CreateCategoryCommandHandler).Assembly, // Assembly dos handlers de Category
        typeof(CreateProductCommandHandler).Assembly, // Assembly dos handlers de Product
        typeof(Program).Assembly // Assembly atual (se necessário)
    );
});
Console.WriteLine("🎯 [5] MediatR configured with ALL handlers!");

// Controllers
builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context => new BadRequestObjectResult(
            context.ModelState
        );
    });

Console.WriteLine("🎮 [6] Controllers added!");

// HttpClient
builder.Services.AddHttpClient();

// Infrastructure services (MongoDB e repositories)
Console.WriteLine("⚠️ [7] About to add Infrastructure services (MongoDB)...");
builder.Services.AddInfrastructure(builder.Configuration);
Console.WriteLine("✅ [8] Infrastructure services added!");

// Application services
builder.Services.AddApplicationServices();
Console.WriteLine("✅ [9] Application services added!");

// AutoMapper
builder.Services.AddAutoMapper(typeof(ProductProfile).Assembly);
Console.WriteLine("✅ [10] AutoMapper configured!");

// Logging detalhado
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Problem details para exceções
builder.Services.AddProblemDetails();

// CORS
var corsSettings = builder.Configuration.GetSection("Cors").Get<CorsSettings>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowSpecificOrigins",
        policy =>
        {
            if (corsSettings?.AllowedOrigins != null && corsSettings.AllowedOrigins.Any())
            {
                policy
                    .WithOrigins(corsSettings.AllowedOrigins.ToArray())
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }
        }
    );
});

Console.WriteLine("🌐 [11] CORS configured!");

// Health checks
builder.Services.AddHealthChecks();
Console.WriteLine("✅ [12] Health checks added!");

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "Hypesoft Challenge X API",
            Version = "v1",
            Description = "API for managing products with JWT authentication",
            Contact = new OpenApiContact { Name = "Hypesoft Team", Email = "dev@hypesoft.com" },
            License = new OpenApiLicense
            {
                Name = "MIT License",
                Url = new Uri("https://opensource.org/licenses/MIT"),
            },
        }
    );

    // JWT Authentication para Swagger
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter JWT Bearer token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme,
        },
    };

    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement { { securityScheme, Array.Empty<string>() } }
    );

    // XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    c.EnableAnnotations();
});

Console.WriteLine("📚 [13] Swagger configured!");

// JWT Authentication
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "http://localhost:8080/realms/hypesoft-realm";
        options.Audience = "hypesoftx-api";
        options.RequireHttpsMetadata = false;
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "http://localhost:8080/realms/hypesoft-realm",
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RoleClaimType = "roles",
            NameClaimType = "preferred_username",
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"🚨 JWT AUTH FAILED: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine(
                    $"🚨 JWT CHALLENGE: {context.Error} - {context.ErrorDescription}"
                );
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                var token = context.Request.Headers["Authorization"].ToString();
                Console.WriteLine($"🔍 TOKEN RECEBIDO: {token}");
                return Task.CompletedTask;
            },
        };
    });

Console.WriteLine("🔐 [14] JWT Authentication configured!");

// Claims transformation
builder.Services.AddScoped<IClaimsTransformation, KeycloakRolesTransformation>();

Console.WriteLine("⚙️ [15] All services configured, building app...");

// Build da aplicação
var app = builder.Build();
Console.WriteLine("🏗️ [16] App built successfully!");

// Exception handling middleware (PRIMEIRO)
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var logger = app.Services.GetRequiredService<ILogger<Program>>();

        if (exceptionFeature?.Error != null)
        {
            Console.WriteLine();
            Console.WriteLine("🚨🚨🚨 EXCEÇÃO DETALHADA 500 🚨🚨🚨");
            Console.WriteLine($"💥 TIPO: {exceptionFeature.Error.GetType().Name}");
            Console.WriteLine($"📝 MENSAGEM: {exceptionFeature.Error.Message}");
            Console.WriteLine($"📍 STACK TRACE:");
            Console.WriteLine(exceptionFeature.Error.StackTrace);

            if (exceptionFeature.Error.InnerException != null)
            {
                Console.WriteLine(
                    $"🔍 INNER EXCEPTION: {exceptionFeature.Error.InnerException.Message}"
                );
                Console.WriteLine(
                    $"📍 INNER STACK: {exceptionFeature.Error.InnerException.StackTrace}"
                );
            }

            Console.WriteLine("🚨🚨🚨 FIM DA EXCEÇÃO 🚨🚨🚨");
            Console.WriteLine();

            logger.LogError(exceptionFeature.Error, "Unhandled Exception in API");
        }

        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var problemDetails = new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            title = "Internal Server Error",
            status = 500,
            detail = exceptionFeature?.Error?.Message ?? "An unexpected error occurred.",
            instance = context.Request.Path.ToString(),
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    });
});

// Request/Response logging middleware
app.Use(
    async (context, next) =>
    {
        Console.WriteLine($"🔥 REQUEST: {context.Request.Method} {context.Request.Path}");
        Console.WriteLine($"📋 Query: {context.Request.QueryString}");
        Console.WriteLine($"🌐 ContentType: {context.Request.ContentType}");
        Console.WriteLine($"📏 ContentLength: {context.Request.ContentLength}");

        try
        {
            await next.Invoke();
            Console.WriteLine($"✅ RESPONSE: {context.Response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🚨 EXCEÇÃO CAPTURADA GLOBALMENTE:");
            Console.WriteLine($"📋 TIPO: {ex.GetType().Name}");
            Console.WriteLine($"💬 MENSAGEM: {ex.Message}");
            Console.WriteLine($"📍 STACK TRACE: {ex.StackTrace}");

            if (ex.InnerException != null)
            {
                Console.WriteLine($"🔍 INNER EXCEPTION: {ex.InnerException.Message}");
            }

            Console.WriteLine("========================================");
            throw;
        }

        Console.WriteLine("----------------------------------------");
    }
);

// Pipeline configuration
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hypesoft Challenge X API v1");
    c.RoutePrefix = "swagger";
});

Console.WriteLine("🛠️ [17] Pipeline configured!");

app.UseCors("AllowSpecificOrigins");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

Console.WriteLine("🔧 [18] Middleware configured!");

// ===============================================
// 🧪 ENDPOINTS DE TESTE (mantidos para debug)
// ===============================================

app.MapGet(
        "/debug/claims",
        (HttpContext ctx) =>
        {
            var roles = ctx
                .User.Claims.Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToArray();
            var all = ctx.User.Claims.Select(c => new { c.Type, c.Value }).ToArray();
            return Results.Ok(new { roles, all });
        }
    )
    .RequireAuthorization();

app.MapGet(
    "/api/test-simple",
    () =>
    {
        Console.WriteLine("🔥 MINIMAL API FUNCIONANDO!");
        return Results.Ok(new { message = "API está funcionando!", timestamp = DateTime.UtcNow });
    }
);

app.MapGet(
    "/api/test-mongodb",
    async ([FromServices] ApplicationDbContext context) =>
    {
        try
        {
            Console.WriteLine("🔥 TESTANDO MONGODB...");
            var productCount = await context.Products.CountAsync();
            var categoryCount = await context.Categories.CountAsync();
            var userCount = await context.Users.CountAsync();

            Console.WriteLine(
                $"✅ MongoDB funcionando! Products: {productCount}, Categories: {categoryCount}, Users: {userCount}"
            );

            return Results.Ok(
                new
                {
                    message = "MongoDB conectado com sucesso!",
                    database = "HypesoftDb",
                    collections = new
                    {
                        products = productCount,
                        categories = categoryCount,
                        users = userCount,
                    },
                    timestamp = DateTime.UtcNow,
                }
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🚨 ERRO MONGODB: {ex.Message}");
            return Results.Problem($"Erro MongoDB: {ex.Message}");
        }
    }
);

app.MapGet(
    "/api/test-mediatr",
    async ([FromServices] IMediator mediator) =>
    {
        try
        {
            Console.WriteLine("🔥 TESTANDO MEDIATR...");
            Console.WriteLine("✅ MediatR injetado com sucesso!");

            return Results.Ok(
                new
                {
                    message = "MediatR funcionando!",
                    mediatorType = mediator.GetType().Name,
                    timestamp = DateTime.UtcNow,
                }
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🚨 ERRO MEDIATR: {ex.Message}");
            return Results.Problem($"Erro MediatR: {ex.Message}");
        }
    }
);

app.MapGet(
    "/api/test-repositories",
    async (
        [FromServices] IProductRepository productRepo,
        [FromServices] ICategoryRepository categoryRepo
    ) =>
    {
        try
        {
            Console.WriteLine("🔥 TESTANDO REPOSITORIES...");

            if (productRepo == null)
            {
                Console.WriteLine("🚨 ERRO: ProductRepository é NULL");
                return Results.Problem("ProductRepository não foi registrado no DI");
            }

            if (categoryRepo == null)
            {
                Console.WriteLine("🚨 ERRO: CategoryRepository é NULL");
                return Results.Problem("CategoryRepository não foi registrado no DI");
            }

            Console.WriteLine("✅ Repositórios injetados com sucesso!");

            var products = await productRepo.GetAllAsync(1, 5, null);
            var productsCount = products?.Count() ?? 0;
            Console.WriteLine($"✅ ProductRepository retornou {productsCount} produtos");

            var categories = await categoryRepo.GetAllAsync();
            var categoriesCount = categories?.Count() ?? 0;
            Console.WriteLine($"✅ CategoryRepository retornou {categoriesCount} categorias");

            return Results.Ok(
                new
                {
                    message = "Repositories funcionando!",
                    productsCount,
                    categoriesCount,
                    timestamp = DateTime.UtcNow,
                }
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🚨 ERRO em métodos dos repositórios: {ex.Message}");
            return Results.Problem($"Erro nos repositórios: {ex.Message}");
        }
    }
);

app.MapGet(
    "/api/test-health",
    () =>
    {
        return Results.Ok(
            new
            {
                status = "Healthy",
                message = "API respondendo normalmente",
                timestamp = DateTime.UtcNow,
                environment = app.Environment.EnvironmentName,
            }
        );
    }
);

// 6. Teste específico do CreateProductCommand
app.MapPost(
    "/api/test-create-product-command",
    async ([FromServices] IMediator mediator) =>
    {
        try
        {
            Console.WriteLine("🔥 TESTANDO CreateProductCommand...");

            // ✅ CORREÇÃO: Use object initializer sem construtor com parâmetros
            var command = new CreateProductCommand(
                "Teste Produto", // Name (obrigatório)
                "Produto de teste", // Description (obrigatório)
                100.00m, // Price (obrigatório)
                Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), // CategoryId (obrigatório)
                "TEST-001", // Sku (opcional)
                "123456789", // Barcode (opcional)
                90.00m, // DiscountPrice (opcional)
                10, // StockQuantity (opcional)
                "https://example.com/image.jpg", // ImageUrl (opcional)
                true, // IsFeatured (opcional)
                true, // IsPublished (opcional)
                "admin" // CreatedBy (opcional)
            );

            Console.WriteLine("✅ Command criado, enviando via MediatR...");
            var result = await mediator.Send(command);
            Console.WriteLine($"✅ Resultado: {result}");

            return Results.Ok(
                new
                {
                    message = "CreateProductCommand funcionou!",
                    result = result,
                    timestamp = DateTime.UtcNow,
                }
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🚨 ERRO CreateProductCommand: {ex.Message}");
            Console.WriteLine($"🚨 STACK: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"🚨 INNER: {ex.InnerException.Message}");
            }
            return Results.Problem($"Erro CreateProductCommand: {ex.Message}");
        }
    }
);

// 6. Teste específico do CreateProductCommand
app.MapPost(
    "/api/test-create-product-command",
    async ([FromServices] IMediator mediator) =>
    {
        try
        {
            Console.WriteLine("🔥 TESTANDO CreateProductCommand...");

            // ✅ CORREÇÃO: Use o construtor com APENAS os parâmetros obrigatórios
            var command = new CreateProductCommand(
                Name: "Teste Produto", // string Name (obrigatório)
                Description: "Produto de teste", // string? Description (obrigatório)
                Price: 100.00m, // decimal Price (obrigatório)
                CategoryId: Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), // Guid CategoryId (obrigatório)
                Sku: "TEST-001", // string? Sku (opcional)
                Barcode: "123456789", // string? Barcode (opcional)
                DiscountPrice: 90.00m, // decimal? DiscountPrice (opcional)
                StockQuantity: 10, // int StockQuantity (opcional)
                ImageUrl: "https://example.com/image.jpg", // string? ImageUrl (opcional)
                IsFeatured: true, // bool IsFeatured (opcional)
                IsPublished: true, // bool IsPublished (opcional)
                CreatedBy: "admin" // string? CreatedBy (opcional)
            );

            Console.WriteLine("✅ Command criado, enviando via MediatR...");
            var result = await mediator.Send(command);
            Console.WriteLine($"✅ Resultado: {result}");

            return Results.Ok(
                new
                {
                    message = "CreateProductCommand funcionou!",
                    result = result,
                    timestamp = DateTime.UtcNow,
                }
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🚨 ERRO CreateProductCommand: {ex.Message}");
            Console.WriteLine($"🚨 STACK: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"🚨 INNER: {ex.InnerException.Message}");
            }
            return Results.Problem($"Erro CreateProductCommand: {ex.Message}");
        }
    }
);

// Map controllers e health checks
app.MapControllers();
app.MapHealthChecks("/health");

Console.WriteLine("🛣️ [19] Endpoints mapped!");

// URLs
app.Urls.Clear();
app.Urls.Add("http://localhost:5010");
Console.WriteLine("🎯 [20] URLs configured: http://localhost:5010");

try
{
    Console.WriteLine("🚀 [21] Starting Kestrel server...");
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"❌ [ERROR] Failed to start server: {ex.Message}");
}

Console.WriteLine("✅ [22] Server stopped successfully!");

public partial class Program { }

public class CorsSettings
{
    public List<string> AllowedOrigins { get; set; } = new();
}

public class KeycloakRolesTransformation : IClaimsTransformation
{
    private const string ClientId = "hypesoftx-api";

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity)
            return Task.FromResult(principal);

        if (identity.Claims.Any(c => c.Type == ClaimTypes.Role))
            return Task.FromResult(principal);

        // Client roles: resource_access.hypesoftx-api.roles
        var resourceAccessJson = identity.FindFirst("resource_access")?.Value;
        if (!string.IsNullOrEmpty(resourceAccessJson))
        {
            using var doc = JsonDocument.Parse(resourceAccessJson);
            if (
                doc.RootElement.TryGetProperty(ClientId, out var clientObj)
                && clientObj.TryGetProperty("roles", out var rolesElem)
                && rolesElem.ValueKind == JsonValueKind.Array
            )
            {
                foreach (var r in rolesElem.EnumerateArray())
                {
                    var role = r.GetString();
                    if (!string.IsNullOrWhiteSpace(role))
                        identity.AddClaim(new Claim(ClaimTypes.Role, role));
                }
            }
        }

        // Realm roles: realm_access.roles
        var realmAccessJson = identity.FindFirst("realm_access")?.Value;
        if (!string.IsNullOrEmpty(realmAccessJson))
        {
            using var doc2 = JsonDocument.Parse(realmAccessJson);
            if (
                doc2.RootElement.TryGetProperty("roles", out var realmRoles)
                && realmRoles.ValueKind == JsonValueKind.Array
            )
            {
                foreach (var r in realmRoles.EnumerateArray())
                {
                    var role = r.GetString();
                    if (!string.IsNullOrWhiteSpace(role))
                        identity.AddClaim(new Claim(ClaimTypes.Role, role));
                }
            }
        }

        return Task.FromResult(principal);
    }
}
