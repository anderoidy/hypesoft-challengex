namespace Hypesoft.Application.DTOs
{
    public class ProductsReportDto
    {
        public List<ProductReportItemDto> Products { get; set; } = new();
        public decimal TotalValue { get; set; }
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
        public Dictionary<string, int> ProductsByCategory { get; set; } = new();
    }

    public class ProductReportItemDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Status { get; set; }
    }
}
