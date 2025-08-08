using Hypesoft.Domain.Common.Interfaces;

namespace Hypesoft.Domain.Repositories;

public interface INamedEntityRepository<TEntity> : IRepository<TEntity> 
    where TEntity : class, INamedEntity
{
    /// <summary>
    /// Verifies if a name is unique in the repository
    /// </summary>
    /// <param name="name">The name to check</param>
    /// <param name="excludeId">Optional ID to exclude from the check (useful for updates)</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete</param>
    /// <returns>True if the name is unique, false otherwise</returns>
    Task<bool> IsNameUniqueAsync(
        string name, 
        Guid? excludeId = null, 
        CancellationToken cancellationToken = default);
}
