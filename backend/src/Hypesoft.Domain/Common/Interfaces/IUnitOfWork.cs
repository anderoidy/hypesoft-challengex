namespace Hypesoft.Domain.Common.Interfaces;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    // Repositórios
    IRepository<TEntity> Repository<TEntity>() where TEntity : EntityBase;
    
    // Controle de transações
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    
    // Salvar alterações
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    // Reverter alterações não salvas
    void RejectChanges();
    
    // Rastreamento de entidades
    void DetachAllEntities();
    
    // Execução de comandos SQL
    Task<int> ExecuteSqlCommandAsync(string sql, params object[] parameters);
    
    // Verificação de conexão
    Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);
}
