using Hypesoft.Domain.Common;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Entities; // Adicione este using
using System.Threading;
using System.Threading.Tasks;

namespace Hypesoft.Domain.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<bool> IsSkuUniqueAsync(string sku, CancellationToken cancellationToken = default);
    Task<bool> IsBarcodeUniqueAsync(string barcode, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}