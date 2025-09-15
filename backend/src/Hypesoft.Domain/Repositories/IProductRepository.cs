using Hypesoft.Domain.Entities;

namespace Hypesoft.Domain.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null
        );
        Task<Product?> GetByIdAsync(Guid id);
        Task<int> GetTotalCountAsync(string? search = null);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(Guid id);
    }
}
