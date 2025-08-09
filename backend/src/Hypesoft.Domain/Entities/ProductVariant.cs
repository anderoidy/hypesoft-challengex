using Hypesoft.Domain.Common;
using Hypesoft.Domain.Exceptions;



namespace Hypesoft.Domain.Entities;

public class ProductVariant : EntityBase
{
    public string Name { get; private set; }  // Ex: "Pequeno", "Azul", "Algodão"
    public string? Description { get; private set; }
    public string? Sku { get; private set; }
    public string? Barcode { get; private set; }
    public decimal PriceAdjustment { get; private set; }  // Pode ser positivo ou negativo
    public decimal? WeightAdjustment { get; private set; }
    public int StockQuantity { get; private set; }
    public bool IsDefault { get; private set; }
    public int DisplayOrder { get; private set; }
    public string? ImageUrl { get; private set; }
    
    // Relacionamentos
    public Guid ProductId { get; private set; }
    public virtual Product Product { get; private set; } = null!;
    
    // Construtor privado para o EF Core
    protected ProductVariant() { }

    public ProductVariant(
        string name,
        Guid productId,
        decimal priceAdjustment = 0,
        decimal? weightAdjustment = null,
        int stockQuantity = 0,
        string? description = null,
        string? sku = null,
        string? barcode = null,
        bool isDefault = false,
        int displayOrder = 0,
        string? imageUrl = null,
        string? userId = null)
    {
        SetName(name);
        SetProductId(productId);
        SetPriceAdjustment(priceAdjustment);
        SetWeightAdjustment(weightAdjustment);
        SetStockQuantity(stockQuantity);
        Description = description?.Trim();
        SetSku(sku);
        SetBarcode(barcode);
        IsDefault = isDefault;
        SetDisplayOrder(displayOrder);
        ImageUrl = imageUrl;
        
        if (userId != null)
            SetCreatedBy(userId);
    }

    // Métodos de negócio
    public void Update(
        string name,
        decimal? priceAdjustment = null,
        decimal? weightAdjustment = null,
        int? stockQuantity = null,
        string? description = null,
        string? sku = null,
        string? barcode = null,
        bool? isDefault = null,
        int? displayOrder = null,
        string? imageUrl = null,
        string? userId = null)
    {
        SetName(name);
        
        if (priceAdjustment.HasValue)
            SetPriceAdjustment(priceAdjustment.Value);
            
        if (weightAdjustment.HasValue)
            SetWeightAdjustment(weightAdjustment);
            
        if (stockQuantity.HasValue)
            SetStockQuantity(stockQuantity.Value);
            
        if (description != null)
            Description = description.Trim();
            
        if (sku != null)
            SetSku(sku);
            
        if (barcode != null)
            SetBarcode(barcode);
            
        if (isDefault.HasValue)
            IsDefault = isDefault.Value;
            
        if (displayOrder.HasValue)
            SetDisplayOrder(displayOrder.Value);
            
        if (imageUrl != null)
            ImageUrl = imageUrl;

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

    public void SetAsDefault(string? userId = null)
    {
        if (IsDefault)
            return;
            
        IsDefault = true;
        
        if (userId != null)
            UpdateAuditFields(userId);
    }

    public void RemoveAsDefault(string? userId = null)
    {
        if (!IsDefault)
            return;
            
        IsDefault = false;
        
        if (userId != null)
            UpdateAuditFields(userId);
    }

    // Métodos internos para ser usado apenas pela entidade Product
    internal void SetProduct(Product product)
    {
        Product = product ?? throw new ArgumentNullException(nameof(product));
        ProductId = product.Id;
    }

    // Métodos privados para validação
    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome da variante não pode ser vazio");
            
        Name = name.Trim();
    }

    public void SetProductId(Guid productId)
    {
        if (productId == Guid.Empty)
            throw new DomainException("O ID do produto não pode ser vazio");
            
        ProductId = productId;
    }

    private void SetPriceAdjustment(decimal adjustment)
    {
        PriceAdjustment = adjustment;
    }

    private void SetWeightAdjustment(decimal? adjustment)
    {
        WeightAdjustment = adjustment;
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

    private void SetDisplayOrder(int order)
    {
        if (order < 0)
            throw new DomainException("A ordem de exibição não pode ser negativa");
            
        DisplayOrder = order;
    }
}
