public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    public float? Weight { get; set; }
    public float? Height { get; set; }
    public float? Width { get; set; }
    public float? Length { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
}