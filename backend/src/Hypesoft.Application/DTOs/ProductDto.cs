namespace Hypesoft.Application.DTOs;

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
    bool IsFeatured = false,
    bool IsPublished = false,
    DateTime? PublishedAt = null,
    Guid CategoryId = default,
    string? CategoryName = null)
{
    public bool HasDiscount => DiscountPrice.HasValue && DiscountPrice < Price;
    public decimal CurrentPrice => DiscountPrice ?? Price;
}