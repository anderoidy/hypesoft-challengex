using Hypesoft.Domain.Entities;

namespace Hypesoft.Domain.Repositories
{
    public interface IRoleRepository
    {
        Task<ApplicationRole?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApplicationRole?> GetByNameAsync(
            string name,
            CancellationToken cancellationToken = default
        );
        Task<IEnumerable<ApplicationRole>> GetAllAsync(
            CancellationToken cancellationToken = default
        );
        Task<ApplicationRole> AddAsync(
            ApplicationRole role,
            CancellationToken cancellationToken = default
        );
        Task<ApplicationRole> UpdateAsync(
            ApplicationRole role,
            CancellationToken cancellationToken = default
        );
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

        // ✅ ADICIONE ESTES MÉTODOS FALTANDO:
        Task<IEnumerable<ApplicationRole>> ListAllAsync(
            CancellationToken cancellationToken = default
        );
        Task<ApplicationRole?> GetByIdWithDetailsAsync(
            Guid id,
            CancellationToken cancellationToken = default
        );
    }
}
