using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Hypesoft.Infrastructure.Configurations;
using Hypesoft.Infrastructure.Data;
using Hypesoft.Infrastructure.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

Console.WriteLine("🚀 [1] Starting application...");

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("✅  Builder created successfully!");

// Add configuration
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

// Add services to the container
builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

Console.WriteLine("🎮 [5] Controllers added!");

// Add infrastructure services (inclui MongoDB e repositories)
Console.WriteLine("⚠️   About to add Infrastructure services (MongoDB)...");
builder.Services.AddInfrastructure(builder.Configuration);
Console.WriteLine("✅ [7] Infrastructure services added!");

// Configure CORS
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

Console.WriteLine("🌐 [8] CORS configured!");

// Add basic health checks
Console.WriteLine("⚠️   Adding basic health checks...");
builder.Services.AddHealthChecks();
Console.WriteLine("✅  Health checks added!");

// Configure Swagger
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

    // Add JWT Authentication to Swagger
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

    // Enable XML comments for Swagger
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    c.EnableAnnotations();
});

Console.WriteLine("📚 [11] Swagger configured!");

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings != null)
{
    builder
        .Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Key)
                ),
                ClockSkew = TimeSpan.Zero,
            };
        });
}

Console.WriteLine("🔐 [12] JWT Authentication configured!");

Console.WriteLine("⚙️   All services configured, building app...");

// Build the application
var app = builder.Build();

Console.WriteLine("🏗️  [14] App built successfully!");

// Configure the HTTP request pipeline - SWAGGER SEMPRE ATIVO
app.UseStaticFiles();
app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hypesoft Challenge X API v1");
    c.RoutePrefix = "swagger";
});

Console.WriteLine("🛠️  [15] Pipeline configured!");

// Enable CORS
app.UseCors("AllowSpecificOrigins");

// Enable routing and authentication/authorization
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Add global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

Console.WriteLine("🔧 [16] Middleware configured!");

// Map controllers and health checks
app.MapControllers();
app.MapHealthChecks("/health");

Console.WriteLine("🛣️  [17] Endpoints mapped!");

Console.WriteLine("🌐  About to start listening...");

// Configurar porta disponível
app.Urls.Clear();
app.Urls.Add("http://localhost:5010");

Console.WriteLine("🎯 [19] URLs configured: http://localhost:5010");

try
{
    Console.WriteLine("🚀  Starting Kestrel server...");
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"❌ [ERROR] Failed to start server: {ex.Message}");
}

Console.WriteLine("✅ [21] Server stopped successfully!");

// Make the Program class public for integration testing
public partial class Program { }

// Settings classes
public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationInMinutes { get; set; } = 60;
}

public class CorsSettings
{
    public List<string> AllowedOrigins { get; set; } = new();
}
