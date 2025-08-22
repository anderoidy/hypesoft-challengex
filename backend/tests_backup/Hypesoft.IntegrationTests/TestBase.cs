using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class TestBase : IDisposable
{
    public IServiceProvider ServiceProvider { get; private set; }
    public ApplicationDbContext DbContext { get; private set; }

    public TestBase()
    {
        var services = new ServiceCollection();
        
        // Adicionar logging
        services.AddLogging();
        
        // Configurar banco em memória
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        
        // Registrar repositórios
        services.AddScoped<IUserRepository, UserRepository>();
        
        // Build do ServiceProvider
        ServiceProvider = services.BuildServiceProvider();
        
        // Obter DbContext
        DbContext = ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Criar banco
        DbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        DbContext?.Database.EnsureDeleted();
        DbContext?.Dispose();
    }
}
