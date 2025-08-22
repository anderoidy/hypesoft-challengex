using Ardalis.Specification;
using Hypesoft.Domain.Entities;

namespace Hypesoft.Domain.Specifications
{
    public class ProductBySlugExcludingIdSpecification : Specification<Product>
    {
        public ProductBySlugExcludingIdSpecification(string slug, Guid excludeId)
        {
            Query.Where(p => p.Slug == slug && p.Id != excludeId);
        }
    }
}
