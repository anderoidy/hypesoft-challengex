using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Mongo.Infrastructure;

namespace Hypesoft.Infrastructure.Extensions;

public static class MongoDbOptionsExtensions
{
    public static DbContextOptionsBuilder UseMongoDB(
        this DbContextOptionsBuilder optionsBuilder,
        string connectionString,
        string databaseName)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("A string de conexão não pode ser nula ou vazia.", nameof(connectionString));
        }

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new ArgumentException("O nome do banco de dados não pode ser nulo ou vazio.", nameof(databaseName));
        }

        return optionsBuilder.UseMongoDB(connectionString, databaseName, mongoOptions =>
        {
            // Configurações adicionais do MongoDB podem ser feitas aqui
            // Exemplo: Configurar timeouts, pooling, etc.
        });
    }

    public static MongoOptionsExtension GetMongoOptionsExtension(this DbContextOptions options)
    {
        var extension = options.FindExtension<MongoOptionsExtension>();
        if (extension == null)
        {
            throw new InvalidOperationException($"{nameof(MongoOptionsExtension)} não encontrado para o {nameof(DbContextOptions)} fornecido.");
        }
        return extension;
    }
}
