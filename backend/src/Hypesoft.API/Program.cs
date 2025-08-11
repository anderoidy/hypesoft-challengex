using Hypesoft.Application;
using Hypesoft.Infrastructure;
using Hypesoft.Domain.Exceptions;
using System.Reflection;
using Hypesoft.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Hypesoft.Infrastructure.Auth;
using Microsoft.ApplicationInsights;

var builder = WebApplication.CreateBuilder(args);

// Configure MongoDB Settings
builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection(MongoDBSettings.SectionName));

// Configure JWT Settings
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName));

// Configuração do MediatR
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// Configuração do AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Registrar serviços das camadas Application e Infrastructure
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
if (jwtSettings?.Secret != null)
{
    builder.Services.AddAuthentication(options =>
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
                Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ClockSkew = TimeSpan.Zero
        };
    });
}

// Register JWT Service
builder.Services.AddScoped<IJwtService, JwtService>();

// Register MongoDB migration services
builder.Services.AddScoped<MongoDbMigrator>();

// Add Application Insights telemetry
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.EnableAdaptiveSampling = false; // For development, set to true in production
    options.EnableDependencyTrackingTelemetryModule = true;
    options.EnablePerformanceCounterCollectionModule = true;
    options.EnableEventCounterCollectionModule = true;
    options.EnableDependencyTrackingTelemetryModule = true;
    options.EnableQuickPulseMetricStream = true;
    options.EnableDiagnosticsTelemetryModule = true;
    options.EnableAzureInstanceMetadataTelemetryModule = true;
    options.EnableAppServicesHeartbeatTelemetryModule = true;
    options.EnableHeartbeat = true;
    options.AddAutoCollectedMetricExtractor = true;
    options.RequestCollectionOptions.TrackExceptions = true;
});

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddMongoDb(
        mongodbConnectionString: builder.Configuration.GetConnectionString("MongoDB"),
        name: "mongodb",
        timeout: TimeSpan.FromSeconds(3),
        tags: new[] { "ready" })
    .AddCheck<MongoDbHealthCheck>("mongodb-custom");

// Add custom health check for MongoDB
builder.Services.AddSingleton<MongoDbHealthCheck>();

// Adiciona serviços ao container.
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    });

// Configuração do CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Hypesoft Challenge X API", 
        Version = "v1",
        Description = "API for managing products with JWT authentication",
        Contact = new OpenApiContact
        {
            Name = "Hypesoft Team",
            Email = "dev@hypesoft.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

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
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });

    // Enable XML comments for Swagger
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    
    // Enable annotations for Swagger
    c.EnableAnnotations();
    
    // Add operation filters if needed
    // c.OperationFilter<AddResponseHeadersFilter>();
});

// Configuração do Health Check
builder.Services.AddHealthChecks()
    .AddMongoDb(
        mongodbConnectionString: builder.Configuration.GetSection("MongoDBSettings:ConnectionString").Value ?? "mongodb://localhost:27017",
        name: "mongodb",
        tags: new[] { "ready" });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hypesoft Challenge X API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at the root
        c.DocumentTitle = "Hypesoft Challenge X API Documentation";
        
        // Enable the "Authorize" button in Swagger UI
        c.OAuthClientId("swagger-ui");
        c.OAuthAppName("Swagger UI");
        c.OAuthUsePkce();
    });
    
    // Apply migrations in development
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var migrator = scope.ServiceProvider.GetRequiredService<MongoDbMigrator>();
            await migrator.MigrateAsync();
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating the database.");
        }
    }
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Add Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Add health check endpoints
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = (check) => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = (_) => false
});

// Enable Application Insights telemetry
app.Use(async (context, next) =>
{
    // Track request telemetry
    var requestTelemetry = context.Features.Get<RequestTelemetry>();
    if (requestTelemetry != null)
    {
        // Add custom properties to all requests
        requestTelemetry.Properties["Application"] = "Hypesoft.API";
        requestTelemetry.Properties["Environment"] = app.Environment.EnvironmentName;
    }

    await next();
});

app.MapHealthChecks("/health");

app.Run();

// Make the Program class public for integration testing
public partial class Program { }