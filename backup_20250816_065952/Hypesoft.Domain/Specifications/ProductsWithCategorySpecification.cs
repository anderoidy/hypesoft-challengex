using System;
using System.Linq.Expressions;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Entities;

namespace Hypesoft.Domain.Specifications
{
    /// <summary>
    /// Specification for querying products with category information
    /// </summary>
    public class ProductsWithCategorySpecification : BaseSpecification<Product>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductsWithCategorySpecification"/> class.
        /// </summary>
        /// <param name="searchTerm">Optional search term to filter products by name or description</param>
        /// <param name="categoryId">Optional category ID to filter products by category</param>
        /// <param name="minPrice">Optional minimum price filter</param>
        /// <param name="maxPrice">Optional maximum price filter</param>
        /// <param name="isPublished">Optional filter for published products</param>
        /// <param name="isFeatured">Optional filter for featured products</param>
        /// <param name="pageNumber">Page number for pagination (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="orderBy">Field to order by (name, price, etc.)</param>
        /// <param name="orderDescending">Whether to sort in descending order</param>
        public ProductsWithCategorySpecification(
            string? searchTerm = null,
            Guid? categoryId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            bool? isPublished = null,
            bool? isFeatured = null,
            int pageNumber = 1,
            int pageSize = 10,
            string? orderBy = null,
            bool orderDescending = false)
        {
            // Include related category
            AddInclude(p => p.Category);
            
            // Apply search term filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchTermToLower = searchTerm.Trim().ToLower();
                Criteria = p => 
                    p.Name.ToLower().Contains(searchTermToLower) || 
                    (!string.IsNullOrEmpty(p.Description) && p.Description.ToLower().Contains(searchTermToLower));
            }

            // Apply category filter
            if (categoryId.HasValue && categoryId.Value != Guid.Empty)
            {
                Criteria = Criteria.And(p => p.CategoryId == categoryId.Value);
            }

            // Apply price range filter
            if (minPrice.HasValue)
            {
                Criteria = Criteria.And(p => p.Price >= minPrice.Value);
            }
            
            if (maxPrice.HasValue)
            {
                Criteria = Criteria.And(p => p.Price <= maxPrice.Value);
            }

            // Apply published filter
            if (isPublished.HasValue)
            {
                Criteria = Criteria.And(p => p.IsPublished == isPublished.Value);
            }

            // Apply featured filter
            if (isFeatured.HasValue)
            {
                Criteria = Criteria.And(p => p.IsFeatured == isFeatured.Value);
            }

            // Apply ordering
            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                switch (orderBy.ToLowerInvariant())
                {
                    case "name":
                        if (orderDescending)
                            ApplyOrderByDescending(p => p.Name);
                        else
                            ApplyOrderBy(p => p.Name);
                        break;
                    case "price":
                        if (orderDescending)
                            ApplyOrderByDescending(p => p.Price);
                        else
                            ApplyOrderBy(p => p.Price);
                        break;
                    case "created":
                        if (orderDescending)
                            ApplyOrderByDescending(p => p.CreatedAt);
                        else
                            ApplyOrderBy(p => p.CreatedAt);
                        break;
                    default:
                        // Default ordering by name
                        ApplyOrderBy(p => p.Name);
                        break;
                }
            }
            else
            {
                // Default ordering by name if no order specified
                ApplyOrderBy(p => p.Name);
            }

            // Apply pagination
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize > 100) pageSize = 100; // Limit page size to prevent performance issues
            if (pageSize < 1) pageSize = 10;
            
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
    }
}
