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
using Hypesoft.Domain.Repositories;
using Hypesoft.Infrastructure.Data;
using Hypesoft.Infrastructure.Extensions;
using Keycloak.AuthServices.Authentication;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MongoDB.EntityFrameworkCore.Extensions;
using QuestPDF.Infrastructure;
using Serilog;

// QuestPDF
QuestPDF.Settings.License = LicenseType.Community;

Console.WriteLine("🚀 [1] Starting application...");

// Builder
var builder = WebApplication.CreateBuilder(args);
Console.WriteLine("✅ [2] Builder created successfully!");

// Config
builder
    .Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

Console.WriteLine("📋 [3] Configuration loaded!");

// Serilog
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Host.UseSerilog();
Console.WriteLine("📄 [4] Serilog configured!");

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

// Configurações do MongoDB - usa variável de ambiente ou configuração do appsettings.json
var mongoConnectionString =
    builder.Configuration["MONGO_URI"]
    ?? builder.Configuration["MongoDbSettings:ConnectionString"]
    ?? "mongodb://mongodb:27017/hypesoft_challenge";

if (string.IsNullOrEmpty(mongoConnectionString))
{
    throw new Exception(
        "Configuração do MongoDB não encontrada. Verifique MONGO_URI ou MongoDbSettings:ConnectionString no appsettings.json"
    );
}

// Application services
builder.Services.AddApplicationServices(); // mantenha se precisar
Console.WriteLine("✅ [9] Application services added!");

// Infrastructure services (MongoDB + Repositories)
builder.Services.AddInfrastructure(builder.Configuration);
Console.WriteLine("✅ [9.1] Infrastructure services added!");

// AutoMapper
builder.Services.AddAutoMapper(typeof(ProductProfile).Assembly);
Console.WriteLine("✅ [10] AutoMapper configured!");

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Problem details
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

// Keycloak
builder.Services.AddKeycloakWebApiAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

// Claims transformation
builder.Services.AddScoped<IClaimsTransformation, KeycloakRolesTransformation>();
Console.WriteLine("🔐 [13] Keycloak Authentication configured!");

// Swagger
builder.Services.AddEndpointsApiExplorer();
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

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
    c.EnableAnnotations();
});
Console.WriteLine("📚 [14] Swagger configured!");

Console.WriteLine("⚙️ [15] All services configured, building app...");
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hypesoft Challenge X API v1");
    c.RoutePrefix = "swagger";
});

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionHandlerPathFeature != null)
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogError(
                exceptionHandlerPathFeature.Error,
                "Erro não tratado capturado pelo middleware global"
            );
        }
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync(
            "Erro inesperado ocorreu, consulte os logs para detalhes."
        );
    });
});

// Middleware de logs de requests e responses para debugging detalhado
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
                Console.WriteLine($"🔍 INNER EXCEPTION: {ex.InnerException.Message}");
            Console.WriteLine("========================================");
            throw;
        }
        Console.WriteLine("----------------------------------------");
    }
);

Console.WriteLine("🛠️ [17] Pipeline configured!");

app.UseStaticFiles();
app.UseCors("AllowSpecificOrigins");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

Console.WriteLine("🔧 [18] Middleware configured!");

// Endpoints de teste
app.MapGet(
    "/debug/environment",
    (IWebHostEnvironment env) =>
    {
        return Results.Ok(
            new
            {
                EnvironmentName = env.EnvironmentName,
                IsDevelopment = env.IsDevelopment(),
                ContentRootPath = env.ContentRootPath,
                ApplicationName = env.ApplicationName,
            }
        );
    }
);

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
            Console.WriteLine("🔥 TESTANDO MONGODB (consulta simples)...");
            var hasProducts = await context.Products.AnyAsync();
            var hasCategories = await context.Categories.AnyAsync();
            var hasUsers = await context.Users.AnyAsync();

            Console.WriteLine(
                $"✅ MongoDB simples OK! hasProducts: {hasProducts}, hasCategories: {hasCategories}, hasUsers: {hasUsers}"
            );

            return Results.Ok(
                new
                {
                    message = "MongoDB conectado com sucesso (consulta simples)!",
                    collections = new
                    {
                        hasProducts,
                        hasCategories,
                        hasUsers,
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
                return Results.Problem("ProductRepository não foi registrado no DI");
            if (categoryRepo == null)
                return Results.Problem("CategoryRepository não foi registrado no DI");

            var products = await productRepo.GetAllAsync(1, 5, null);
            var productsCount = products?.Count() ?? 0;
            var categories = await categoryRepo.GetAllAsync();
            var categoriesCount = categories?.Count() ?? 0;

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

app.MapPost(
    "/api/test-create-product-command",
    async ([FromServices] IMediator mediator) =>
    {
        try
        {
            Console.WriteLine("🔥 TESTANDO CreateProductCommand...");
            var command = new CreateProductCommand(
                Name: "Teste Produto",
                Description: "Produto de teste",
                Price: 100.00m,
                CategoryId: Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                Sku: "TEST-001",
                Barcode: "123456789",
                DiscountPrice: 90.00m,
                StockQuantity: 10,
                ImageUrl: "https://example.com/image.jpg",
                IsFeatured: true,
                IsPublished: true,
                CreatedBy: "admin"
            );
            var result = await mediator.Send(command);
            return Results.Ok(
                new
                {
                    message = "CreateProductCommand funcionou!",
                    result,
                    timestamp = DateTime.UtcNow,
                }
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🚨 ERRO CreateProductCommand: {ex.Message}");
            return Results.Problem($"Erro CreateProductCommand: {ex.Message}");
        }
    }
);

// Controllers + health
app.MapControllers();
app.MapHealthChecks("/health");
Console.WriteLine("🛣️ [19] Endpoints mapped!");

// URLs
// app.Urls.Clear();
// if (Environment.GetEnvironmentVariable("ASPNETCORE_URLS") == null)
// {
//     app.Urls.Add("http://localhost:5010");
// }
// Console.WriteLine("🎯 [20] URLs configured: http://localhost:5010");

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
    private const string ClientId = "hypesoft-backend";

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity)
            return Task.FromResult(principal);
        if (identity.Claims.Any(c => c.Type == ClaimTypes.Role))
            return Task.FromResult(principal);

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
