using Ardalis.Specification;
using Hypesoft.Domain.Entities;

namespace Hypesoft.Domain.Specifications
{
    public class ProductBySkuExcludingIdSpecification : Specification<Product>
    {
        public ProductBySkuExcludingIdSpecification(string sku, Guid excludeId)
        {
            Query.Where(p => p.Sku == sku && p.Id != excludeId);
        }
    }
}
