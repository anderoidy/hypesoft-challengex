using Ardalis.Specification;
using Hypesoft.Domain.Entities;

#pragma warning disable IDE0130 // O namespace não corresponde à estrutura da pasta
namespace Hypesoft.Domain.Specifications
#pragma warning restore IDE0130 // O namespace não corresponde à estrutura da pasta
{
    public class OutOfStockProductsSpecification : Specification<Product>
    {
        public OutOfStockProductsSpecification()
        {
            Query.Where(p => p.StockQuantity <= 0);
        }
    }
}