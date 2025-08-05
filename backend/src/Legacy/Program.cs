using Hypesoft.API.Extensions;
using Hypesoft.Application; // Adicione este using
using Hypesoft.Infrastructure; // Adicione este using

var builder = WebApplication.CreateBuilder(args);

// Configuracao do AutoMapper
builder.Services.AddAutoMapper(typeof(ProductProfile).Assembly);

// **ADICIONAR: Configuração do MediatR**
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(Application.AssemblyReference).Assembly));

// **ADICIONAR: Registrar serviços das camadas Application e Infrastructure**
builder.Services.AddApplicationServices(); // Método de extensão da Application
builder.Services.AddInfrastructureServices(builder.Configuration); // Método de extensão da Infrastructure

// Adiciona serviços ao container.
builder.Services.AddControllers();

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

// Configuração do Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Hypesoft API", Version = "v1" });
    
    // Configuração para suporte a autenticação JWT no Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configuração do Keycloak
builder.Services.AddKeycloakAuthentication(builder.Configuration);

// Configuração do Health Check
builder.Services.AddHealthChecks()
    .AddMongoDb(
        mongodbConnectionString: builder.Configuration.GetConnectionString("MongoDB"),
        name: "mongodb",
        tags: new[] { "ready" });

var app = builder.Build();

// Configuração do pipeline de requisições HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hypesoft API V1");
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Adiciona autenticação e autorização
app.UseKeycloakAuthentication();

app.MapControllers();

// Configuração dos endpoints de health check
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new()
{
    Predicate = (check) => check.Tags.Contains("ready")
});

app.Run();