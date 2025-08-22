// ProductBySlugSpecification.cs
using Ardalis.Specification;
using Hypesoft.Domain.Entities;

namespace Hypesoft.Domain.Specifications
{
    public class ProductBySlugSpecification : Specification<Product>
    {
        public ProductBySlugSpecification(string slug)
        {
            Query.Where(p => p.Slug == slug);
        }
    }
}

// ProductBySkuSpecification.cs
public class ProductBySkuSpecification : Specification<Product>
{
    public ProductBySkuSpecification(string sku)
    {
        Query.Where(p => p.Sku == sku);
    }
}

// ProductsWithCategorySpecification.cs
public class ProductsWithCategorySpecification : Specification<Product>
{
    public ProductsWithCategorySpecification(Guid categoryId)
    {
        Query.Where(p => p.CategoryId == categoryId);
    }
}

// FeaturedProductsSpecification.cs
public class FeaturedProductsSpecification : Specification<Product>
{
    public FeaturedProductsSpecification(int count)
    {
        Query.Where(p => p.IsFeatured).Take(count);
    }
}

// PaginatedProductsSpecification.cs
public class PaginatedProductsSpecification : Specification<Product>
{
    public PaginatedProductsSpecification(int pageNumber, int pageSize)
    {
        Query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    }
}

// ProductSearchSpecification.cs
public class ProductSearchSpecification : Specification<Product>
{
    public ProductSearchSpecification(string searchTerm, int pageNumber, int pageSize)
    {
        Query
            .Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}
