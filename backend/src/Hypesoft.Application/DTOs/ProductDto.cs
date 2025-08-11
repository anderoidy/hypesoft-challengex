// Before
namespace Hypesoft.Application.DTOs
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        // ... other properties
    }
}
// After
public record ProductDto(
    Guid Id,
    string Name,
    string? Description = null,
    string? ImageUrl = null,
    decimal Price = 0,
    decimal? DiscountPrice = null,
    int StockQuantity = 0,
    string? Sku = null,
    string? Barcode = null,
    ProductDimensionsDto? Dimensions = null,
    bool IsFeatured = false,
    bool IsPublished = false,
    DateTime? PublishedAt = null,
    Guid CategoryId = default,
    string? CategoryName = null)
{
    public bool HasDiscount => DiscountPrice.HasValue && DiscountPrice < Price;
    public decimal CurrentPrice => DiscountPrice ?? Price;
}

public record ProductDimensionsDto(
    float? Weight = null,
    float? Height = null,
    float? Width = null,
    float? Length = null);