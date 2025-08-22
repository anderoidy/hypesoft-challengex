using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification.EntityFrameworkCore;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Specifications;
using Hypesoft.Infrastructure.Data;

namespace Hypesoft.Infrastructure.Repositories
{
    public class ProductRepository : RepositoryBase<Product>
    {
        public ProductRepository(ApplicationDbContext context)
            : base(context) { }

        public async Task<Product?> GetBySkuAsync(
            string sku,
            CancellationToken cancellationToken = default
        )
        {
            var spec = new GetProductBySkuSpec(sku);
            return await FirstOrDefaultAsync(spec, cancellationToken);
        }

        public async Task<Product?> GetBySlugAsync(
            string slug,
            CancellationToken cancellationToken = default
        )
        {
            // Implementação simples sem specification por enquanto
            var allProducts = await ListAsync(cancellationToken);
            return allProducts.FirstOrDefault(p => p.Slug == slug);
        }

        public async Task<bool> IsSkuUniqueAsync(
            string sku,
            Guid? excludeId = null,
            CancellationToken cancellationToken = default
        )
        {
            // Implementação simples sem specification por enquanto
            var allProducts = await ListAsync(cancellationToken);

            var hasExisting = excludeId.HasValue
                ? allProducts.Any(p => p.Sku == sku && p.Id != excludeId.Value)
                : allProducts.Any(p => p.Sku == sku);

            return !hasExisting;
        }
    }
}
