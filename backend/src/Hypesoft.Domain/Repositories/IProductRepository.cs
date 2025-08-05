using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Entities;

namespace Hypesoft.Domain.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<bool> IsSkuUniqueAsync(string sku, CancellationToken cancellationToken = default);
    Task<bool> IsBarcodeUniqueAsync(string barcode, CancellationToken cancellationToken = default);
    // Adicione métodos específicos de consulta de produto conforme necessário
}
