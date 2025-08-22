using Ardalis.Specification;
using Hypesoft.Domain.Entities;

namespace Hypesoft.Domain.Specifications
{
    public class AllProductsSpecification : Specification<Product>
    {
        public AllProductsSpecification()
        {
            Query.OrderBy(p => p.Name);
        }
    }
}
