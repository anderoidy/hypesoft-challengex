using Hypesoft.Domain.Entities;

namespace Hypesoft.Domain.Repositories
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default);
        Task<Category> UpdateAsync(
            Category category,
            CancellationToken cancellationToken = default
        );
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default); // Deleta por ID
        Task DeleteAsync(Category category, CancellationToken cancellationToken = default); // Deleta por entidade
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> HasSubCategoriesAsync(
            Guid categoryId,
            CancellationToken cancellationToken = default
        );
        Task<bool> HasProductsAsync(Guid categoryId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Category>> GetMainCategoriesAsync(
            CancellationToken cancellationToken = default
        );
    }
}
