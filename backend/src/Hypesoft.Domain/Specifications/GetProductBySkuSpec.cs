using Ardalis.Specification;
using Hypesoft.Domain.Entities;

namespace Hypesoft.Domain.Specifications
{
    public class GetProductBySkuSpec : Specification<Product>
    {
        public GetProductBySkuSpec(string sku)
        {
            Query.Where(product => product.Sku == sku);
        }
    }
}
