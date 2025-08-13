using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Hypesoft.API;
using Hypesoft.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Hypesoft.IntegrationTests;

public class TestBase : IDisposable
{
    protected readonly HttpClient TestClient;
    protected readonly ApplicationDbContext DbContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly WebApplicationFactory<Program> _factory;

    protected TestBase()
    {
        // Configura o host de teste com um banco de dados em memória
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration(
                (context, config) => {
                    // Adiciona configurações de teste, se necessário
                }
            );

            builder.ConfigureServices(services =>
            {
                // Remove o DbContext existente
                services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));

                // Adiciona o DbContext com um banco de dados em memória
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                    options.EnableSensitiveDataLogging();
                });

                // Configura o Identity com um banco de dados em memória
                services
                    .AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();

                // Configura políticas de autorização, se necessário
                services.AddAuthorization();

                // Configura o contexto HTTP para testes
                var serviceProvider = services.BuildServiceProvider();

                // Cria o banco de dados e aplica as migrações
                using (var scope = serviceProvider.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<ApplicationDbContext>();
                    var logger = scopedServices.GetRequiredService<ILogger<TestBase>>();

                    // Garante que o banco de dados seja criado
                    db.Database.EnsureCreated();

                    try
                    {
                        // Aplica as migrações
                        db.Database.Migrate();

                        // Inicializa o banco de dados com dados de teste, se necessário
                        // SeedTestData.InitializeDbForTests(db);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(
                            ex,
                            "Ocorreu um erro ao configurar o banco de dados de teste: {Message}",
                            ex.Message
                        );
                    }
                }
            });

            builder.UseEnvironment("Test");
        });

        // Cria o cliente HTTP para testes
        TestClient = _factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }
        );

        // Configura o cliente HTTP com headers padrão, se necessário
        TestClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );

        // Obtém o DbContext para uso nos testes
        var scope = _factory.Services.CreateScope();
        _serviceProvider = scope.ServiceProvider;
        DbContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();
    }

    protected async Task AuthenticateAsync()
    {
        // Ensure the test user exists
        var userManager = _serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = _serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var configuration = _serviceProvider.GetRequiredService<IConfiguration>();

        // Create test role if it doesn't exist
        const string testRole = "TestRole";
        if (!await roleManager.RoleExistsAsync(testRole))
        {
            await roleManager.CreateAsync(new IdentityRole(testRole));
        }

        // Create test user if it doesn't exist
        var testUser = await userManager.FindByEmailAsync("test@example.com");
        if (testUser == null)
        {
            testUser = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                EmailConfirmed = true,
            };

            var createUserResult = await userManager.CreateAsync(testUser, "Test@123");
            if (!createUserResult.Succeeded)
            {
                throw new Exception(
                    $"Failed to create test user: {string.Join(", ", createUserResult.Errors)}"
                );
            }

            // Add user to test role
            await userManager.AddToRoleAsync(testUser, testRole);
        }

        // Generate JWT token
        var jwtService = _serviceProvider.GetRequiredService<IJwtService>();
        var roles = await userManager.GetRolesAsync(testUser);
        var token = jwtService.GenerateToken(testUser, roles);

        // Set the authorization header
        TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );
    }

    protected async Task<HttpResponseMessage> CreateProductAsync(object createProductCommand)
    {
        // Método auxiliar para criar um produto nos testes
        return await TestClient.PostAsJsonAsync("/api/products", createProductCommand);
    }

    public void Dispose()
    {
        // Limpa os recursos após cada teste
        TestClient.Dispose();
        DbContext.Database.EnsureDeleted();
        DbContext.Dispose();
        _factory.Dispose();
        GC.SuppressFinalize(this);
    }
}
