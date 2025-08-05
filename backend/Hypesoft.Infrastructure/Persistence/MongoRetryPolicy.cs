using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Polly;

namespace Hypesoft.Infrastructure.Persistence;

public static class MongoRetryPolicy
{
    public static async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> action,
        ILogger logger,
        int retryCount = 3,
        int initialRetryDelayMs = 200,
        CancellationToken cancellationToken = default)
    {
        var retryPolicy = Policy
            .Handle<MongoConnectionException>()
            .Or<TimeoutException>()
            .Or<MongoCommandException>(ex => ex.Code == 16500) // TooManyRequests
            .Or<MongoCommandException>(ex => ex.Code == 16501) // RequestRateTooLarge
            .WaitAndRetryAsync(
                retryCount,
                retryAttempt => 
                {
                    // Exponential backoff com jitter para evitar thundering herd
                    var delay = TimeSpan.FromMilliseconds(
                        initialRetryMs * Math.Pow(2, retryAttempt - 1) * 
                        (0.8 + 0.4 * new Random().NextDouble()));
                    
                    logger?.LogWarning(
                        "Tentativa {RetryAttempt} de {RetryCount} após {Delay}ms. Razão: {Message}",
                        retryAttempt, 
                        retryCount,
                        delay.TotalMilliseconds,
                        "Falha na operação do MongoDB. Tentando novamente...");
                    
                    return delay;
                },
                onRetry: (exception, delay, retryCount, context) =>
                {
                    logger?.LogWarning(
                        exception,
                        "Tentativa {RetryCount} falhou. Tentando novamente em {Delay}ms",
                        retryCount,
                        delay.TotalMilliseconds);
                });

        var circuitBreaker = Policy
            .Handle<MongoConnectionException>()
            .Or<TimeoutException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (ex, breakDelay) =>
                {
                    logger?.LogError(
                        ex,
                        "Circuito quebrado por {BreakDelay}ms devido a falhas repetidas",
                        breakDelay.TotalMilliseconds);
                },
                onReset: () =>
                {
                    logger?.LogInformation("Circuito redefinido, as operações serão retomadas");
                },
                onHalfOpen: () =>
                {
                    logger?.LogInformation("Testando se o serviço se recuperou...");
                });

        // Combina as políticas: primeiro tenta a política de repetição, depois o disjuntor
        var policy = Policy.WrapAsync(circuitBreaker, retryPolicy);

        try
        {
            return await policy.ExecuteAsync(action, cancellationToken);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Falha na execução da operação após {RetryCount} tentativas", retryCount);
            throw;
        }
    }

    public static async Task ExecuteWithRetryAsync(
        Func<Task> action,
        ILogger logger,
        int retryCount = 3,
        int initialRetryMs = 200,
        CancellationToken cancellationToken = default)
    {
        await ExecuteWithRetryAsync(
            async () =>
            {
                await action();
                return true; // Valor fictício para o tipo de retorno
            },
            logger,
            retryCount,
            initialRetryMs,
            cancellationToken);
    }
}
