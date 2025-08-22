using System;
using Ardalis.Specification;
using Hypesoft.Domain.Entities;

namespace Hypesoft.Domain.Specifications
{
    public class ProductsWithCategorySpecification : Specification<Product>
    {
        public ProductsWithCategorySpecification(Guid categoryId)
        {
            Query.Where(p => p.CategoryId == categoryId);
        }
    }
}
