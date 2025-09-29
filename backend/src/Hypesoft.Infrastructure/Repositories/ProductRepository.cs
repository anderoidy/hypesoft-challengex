using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Hypesoft.Domain.Specifications;
using Hypesoft.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Infrastructure.Repositories
{
    public class ProductRepository : BaseRepository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context, ILogger<ProductRepository> logger)
            : base(context, logger)
        {
            _context = context;
        }

        // Implementação explícita da interface para garantir retorno correto
        async Task<IReadOnlyList<Product>> IProductRepository.ListAsync(
            ISpecification<Product> specification,
            CancellationToken cancellationToken
        )
        {
            var list = await base.ListAsync(specification, cancellationToken);
            return list.ToList().AsReadOnly();
        }

        public async Task<IEnumerable<Product>> GetAllAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null
        )
        {
            try
            {
                var query = _context.Products.AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p =>
                        (p.Name != null && p.Name.Contains(search))
                        || (p.Description != null && p.Description.Contains(search))
                    );
                }

                var products = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return products ?? new List<Product>();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Erro ao buscar produtos");
                return new List<Product>();
            }
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.Products.FindAsync(id);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Erro ao buscar produto por ID: {Id}", id);
                return null;
            }
        }

        public async Task<int> GetTotalCountAsync(string? search = null)
        {
            try
            {
                var query = _context.Products.AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p =>
                        (p.Name != null && p.Name.Contains(search))
                        || (p.Description != null && p.Description.Contains(search))
                    );
                }

                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Erro ao contar produtos");
                return 0;
            }
        }

        public async Task AddAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var product = await GetByIdAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Product?> GetBySkuAsync(
            string sku,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                if (string.IsNullOrEmpty(sku))
                    return null;

                var spec = new GetProductBySkuSpec(sku);
                return await FirstOrDefaultAsync(spec, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Erro ao buscar produto por SKU: {Sku}", sku);
                return null;
            }
        }

        public async Task<Product?> GetBySlugAsync(
            string slug,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                if (string.IsNullOrEmpty(slug))
                    return null;

                var allProducts = await ListAsync(cancellationToken);
                return allProducts?.FirstOrDefault(p => p.Slug == slug);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Erro ao buscar produto por Slug", slug);
                return null;
            }
        }

        public async Task<bool> IsSkuUniqueAsync(
            string sku,
            Guid? excludeId = null,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                if (string.IsNullOrEmpty(sku))
                    return false;

                var allProducts = await ListAsync(cancellationToken);

                if (allProducts == null)
                    return true;

                var hasExisting = excludeId.HasValue
                    ? allProducts.Any(p => p.Sku == sku && p.Id != excludeId.Value)
                    : allProducts.Any(p => p.Sku == sku);

                return !hasExisting;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Erro ao verificar unicidade do SKU: {Sku}", sku);
                return false;
            }
        }
    }
}
