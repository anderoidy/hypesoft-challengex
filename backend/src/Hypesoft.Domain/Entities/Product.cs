using Hypesoft.Domain.Common;
using Hypesoft.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Hypesoft.Domain.Entities;

public class Product : EntityBase
{
    public string Name { get; private set; }
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
    
    // Relacionamentos
    public Guid CategoryId { get; private set; }
    public virtual Category Category { get; private set; } = null!;
    public virtual ICollection<ProductVariant> Variants { get; private set; } = new List<ProductVariant>();
    public virtual ICollection<ProductTag> ProductTags { get; private set; } = new List<ProductTag>();

    // Construtor privado para o EF Core
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
        Description = description?.Trim();
        SetPrice(price);
        SetDiscountPrice(discountPrice);
        SetStockQuantity(stockQuantity);
        SetSku(sku);
        SetBarcode(barcode);
        ImageUrl = imageUrl;
        Weight = weight > 0 ? weight : null;
        Height = height > 0 ? height : null;
        Width = width > 0 ? width : null;
        Length = length > 0 ? length : null;
        IsFeatured = isFeatured;
        CategoryId = categoryId;
        
        if (isPublished)
            Publish(userId);
        else
            Unpublish();

        if (userId != null)
            SetCreatedBy(userId);
    }

    // Métodos de negócio
    public void Update(
        string name,
        string? description,
        decimal price,
        Guid categoryId,
        decimal? discountPrice = null,
        int? stockQuantity = null,
        string? sku = null,
        string? barcode = null,
        string? imageUrl = null,
        float? weight = null,
        float? height = null,
        float? width = null,
        float? length = null,
        bool? isFeatured = null,
        string? userId = null)
    {
        SetName(name);
        Description = description?.Trim();
        SetPrice(price);
        
        if (discountPrice.HasValue)
            SetDiscountPrice(discountPrice);
            
        if (stockQuantity.HasValue)
            SetStockQuantity(stockQuantity.Value);
            
        if (sku != null)
            SetSku(sku);
            
        if (barcode != null)
            SetBarcode(barcode);
        
        if (imageUrl != null)
            ImageUrl = imageUrl;
            
        if (weight.HasValue)
            Weight = weight > 0 ? weight : null;
            
        if (height.HasValue)
            Height = height > 0 ? height : null;
            
        if (width.HasValue)
            Width = width > 0 ? width : null;
            
        if (length.HasValue)
            Length = length > 0 ? length : null;
            
        if (isFeatured.HasValue)
            IsFeatured = isFeatured.Value;
            
        CategoryId = categoryId;

        if (userId != null)
            UpdateAuditFields(userId);
    }

    public void Publish(string? userId = null)
    {
        if (IsPublished)
            return;
            
        IsPublished = true;
        PublishedAt = DateTime.UtcNow;
        
        if (userId != null)
            UpdateAuditFields(userId);
    }

    public void Unpublish(string? userId = null)
    {
        if (!IsPublished)
            return;
            
        IsPublished = false;
        PublishedAt = null;
        
        if (userId != null)
            UpdateAuditFields(userId);
    }

    public void AddStock(int quantity, string? userId = null)
    {
        if (quantity <= 0)
            throw new DomainException("A quantidade deve ser maior que zero");
            
        StockQuantity += quantity;
        
        if (userId != null)
            UpdateAuditFields(userId);
    }

    public void RemoveStock(int quantity, string? userId = null)
    {
        if (quantity <= 0)
            throw new DomainException("A quantidade deve ser maior que zero");
            
        if (StockQuantity < quantity)
            throw new DomainException("Quantidade em estoque insuficiente");
            
        StockQuantity -= quantity;
        
        if (userId != null)
            UpdateAuditFields(userId);
    }

    public void SetFeatured(bool isFeatured, string? userId = null)
    {
        IsFeatured = isFeatured;
        
        if (userId != null)
            UpdateAuditFields(userId);
    }

    // Métodos privados para validação
    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome do produto não pode ser vazio");
            
        Name = name.Trim();
    }

    private void SetPrice(decimal price)
    {
        if (price < 0)
            throw new DomainException("O preço não pode ser negativo");
            
        Price = price;
    }

    private void SetDiscountPrice(decimal? discountPrice)
    {
        if (discountPrice.HasValue)
        {
            if (discountPrice.Value < 0)
                throw new DomainException("O preço com desconto não pode ser negativo");
                
            if (discountPrice.Value >= Price)
                throw new DomainException("O preço com desconto deve ser menor que o preço normal");
        }
        
        DiscountPrice = discountPrice;
    }

    private void SetStockQuantity(int quantity)
    {
        if (quantity < 0)
            throw new DomainException("A quantidade em estoque não pode ser negativa");
            
        StockQuantity = quantity;
    }

    private void SetSku(string? sku)
    {
        if (!string.IsNullOrWhiteSpace(sku) && sku.Length > 50)
            throw new DomainException("O SKU não pode ter mais de 50 caracteres");
            
        Sku = sku?.Trim();
    }

    private void SetBarcode(string? barcode)
    {
        if (!string.IsNullOrWhiteSpace(barcode) && barcode.Length > 50)
            throw new DomainException("O código de barras não pode ter mais de 50 caracteres");
            
        Barcode = barcode?.Trim();
    }

    // Métodos para gerenciar variantes
    public void AddVariant(ProductVariant variant, string? userId = null)
    {
        if (variant == null)
            throw new ArgumentNullException(nameof(variant));
            
        if (Variants.Any(v => v.Id == variant.Id))
            return; // Já existe uma variante com este ID
            
        variant.SetProduct(this);
        Variants.Add(variant);
        
        if (userId != null)
            UpdateAuditFields(userId);
    }

    public void RemoveVariant(Guid variantId, string? userId = null)
    {
        var variant = Variants.FirstOrDefault(v => v.Id == variantId);
        if (variant != null)
        {
            Variants.Remove(variant);
            
            if (userId != null)
                UpdateAuditFields(userId);
        }
    }

    // Métodos para gerenciar tags
    public void AddTag(Tag tag, string? userId = null)
    {
        if (tag == null)
            throw new ArgumentNullException(nameof(tag));
            
        if (ProductTags.Any(pt => pt.TagId == tag.Id))
            return; // Já tem esta tag
            
        var productTag = new ProductTag(this, tag);
        ProductTags.Add(productTag);
        
        if (userId != null)
            UpdateAuditFields(userId);
    }

    public void RemoveTag(Guid tagId, string? userId = null)
    {
        var productTag = ProductTags.FirstOrDefault(pt => pt.TagId == tagId);
        if (productTag != null)
        {
            ProductTags.Remove(productTag);
            
            if (userId != null)
                UpdateAuditFields(userId);
        }
    }
}
