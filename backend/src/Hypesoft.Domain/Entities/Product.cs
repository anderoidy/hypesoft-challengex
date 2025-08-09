using Hypesoft.Domain.Common;
using Hypesoft.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Hypesoft.Domain.Entities;

public class Product : EntityBase
{
    // Properties with private setters for encapsulation
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }
    public decimal Price { get; private set; }
    public decimal? DiscountPrice { get; private set; }
    public int StockQuantity { get; private set; }
    public string? Sku { get; private set; }
    public string? Barcode { get; private set; }
    public float? Weight { get; private set; }
    public float? Height { get; private set; }
    public float? Width { get; private set; }
    public float? Length { get; private set; }
    public bool IsFeatured { get; private set; }
    public bool IsPublished { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    
    // Navigation properties
    public Guid CategoryId { get; private set; }
    public virtual Category Category { get; private set; } = null!;
    public virtual ICollection<ProductVariant> Variants { get; private set; } = new List<ProductVariant>();
    public virtual ICollection<ProductTag> ProductTags { get; private set; } = new List<ProductTag>();

    // Private constructor for EF Core
    protected Product() { }

    public Product(
        string name, 
        string? description, 
        decimal price, 
        Guid categoryId, 
        string? sku = null, 
        string? barcode = null,
        decimal? discountPrice = null,
        int stockQuantity = 0,
        string? imageUrl = null,
        float? weight = null,
        float? height = null,
        float? width = null,
        float? length = null,
        bool isFeatured = false,
        bool isPublished = false,
        string? userId = null)
    {
        SetName(name);
        SetDescription(description);
        SetPrice(price);
        SetCategory(categoryId);
        SetSku(sku);
        SetBarcode(barcode);
        SetDiscountPrice(discountPrice);
        SetStockQuantity(stockQuantity);
        SetImageUrl(imageUrl);
        SetDimensions(weight, height, width, length);
        SetIsFeatured(isFeatured);
        
        if (isPublished)
            Publish(userId);
        else
            Unpublish();

        if (userId != null)
            SetCreatedBy(userId);
    }

    // Business methods for updating properties
    public void Update(
        string name,
        string? description,
        decimal price,
        Guid categoryId,
        string? imageUrl = null,
        int? stockQuantity = null,
        decimal? discountPrice = null,
        bool? isFeatured = null,
        string? userId = null)
    {
        SetName(name);
        SetDescription(description);
        SetPrice(price);
        SetCategory(categoryId);
        
        if (imageUrl != null)
            SetImageUrl(imageUrl);
            
        if (stockQuantity.HasValue)
            SetStockQuantity(stockQuantity.Value);
            
        if (discountPrice.HasValue)
            SetDiscountPrice(discountPrice);
            
        if (isFeatured.HasValue)
            SetIsFeatured(isFeatured.Value);
            
        if (userId != null)
            SetLastModifiedBy(userId);
    }

    // Individual property setters with validation
    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name cannot be empty");
            
        Name = name.Trim();
    }
    
    public void SetDescription(string? description)
    {
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }
    
    public void SetPrice(decimal price)
    {
        if (price < 0)
            throw new DomainException("Price cannot be negative");
            
        Price = price;
    }
    
    public void SetDiscountPrice(decimal? discountPrice)
    {
        if (discountPrice.HasValue && discountPrice < 0)
            throw new DomainException("Discount price cannot be negative");
            
        if (discountPrice.HasValue && discountPrice > Price)
            throw new DomainException("Discount price cannot be greater than the regular price");
            
        DiscountPrice = discountPrice;
    }
    
    public void SetStockQuantity(int quantity)
    {
        if (quantity < 0)
            throw new DomainException("Stock quantity cannot be negative");
            
        StockQuantity = quantity;
    }
    
    public void SetSku(string? sku)
    {
        Sku = string.IsNullOrWhiteSpace(sku) ? null : sku.Trim();
    }
    
    public void SetBarcode(string? barcode)
    {
        Barcode = string.IsNullOrWhiteSpace(barcode) ? null : barcode.Trim();
    }
    
    public void SetImageUrl(string? imageUrl)
    {
        // Basic URL validation could be added here
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
    }
    
    public void SetDimensions(float? weight, float? height, float? width, float? length)
    {
        if (weight.HasValue && weight <= 0) throw new DomainException("Weight must be positive");
        if (height.HasValue && height <= 0) throw new DomainException("Height must be positive");
        if (width.HasValue && width <= 0) throw new DomainException("Width must be positive");
        if (length.HasValue && length <= 0) throw new DomainException("Length must be positive");
        
        Weight = weight;
        Height = height;
        Width = width;
        Length = length;
    }
    
    public void SetCategory(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
            throw new DomainException("Category ID cannot be empty");
            
        CategoryId = categoryId;
    }
    
    public void SetIsFeatured(bool isFeatured)
    {
        IsFeatured = isFeatured;
    }
    
    public void Publish(string? userId = null)
    {
        IsPublished = true;
        PublishedAt = DateTime.UtcNow;
        if (userId != null)
            SetLastModifiedBy(userId);
    }
    
    public void Unpublish()
    {
        IsPublished = false;
        PublishedAt = null;
    }
    
    // Additional business methods can be added here
    public void AddVariant(ProductVariant variant)
    {
        if (variant == null) throw new ArgumentNullException(nameof(variant));
        
        if (Variants.Any(v => v.Name.Equals(variant.Name, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException($"A variant with name '{variant.Name}' already exists");
            
        variant.SetProductId(Id);
        Variants.Add(variant);
    }
    
    public void AddTag(Tag tag)
    {
        if (tag == null) throw new ArgumentNullException(nameof(tag));
        
        if (ProductTags.Any(pt => pt.TagId == tag.Id))
            return; // Tag already added
            
        ProductTags.Add(new ProductTag(Id, tag.Id));
    }
    
    public void RemoveTag(Guid tagId)
    {
        var productTag = ProductTags.FirstOrDefault(pt => pt.TagId == tagId);
        if (productTag != null)
            ProductTags.Remove(productTag);
    }
}
