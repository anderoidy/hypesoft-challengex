using Hypesoft.Domain.Common;
using Hypesoft.Domain.Exceptions;

namespace Hypesoft.Domain.Entities;

public class ProductTag : EntityBase
{
    // Chaves estrangeiras
    public Guid ProductId { get; private set; }
    public Guid TagId { get; private set; }
    
    // Propriedades de navegação
    public virtual Product Product { get; private set; } = null!;
    public virtual Tag Tag { get; private set; } = null!;
    
    // Propriedades adicionais do relacionamento
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public bool IsFeatured { get; private set; }
    public int DisplayOrder { get; private set; }
    
    // Construtor privado para o EF Core
    protected ProductTag() { }
    
    // Construtor para criar um ProductTag com Product e Tag completos
    public ProductTag(Product product, Tag tag, bool isFeatured = false, int displayOrder = 0, DateTime? startDate = null, DateTime? endDate = null, string? userId = null)
    {
        Product = product ?? throw new ArgumentNullException(nameof(product));
        ProductId = product.Id;
        
        Tag = tag ?? throw new ArgumentNullException(nameof(tag));
        TagId = tag.Id;
        
        IsFeatured = isFeatured;
        SetDisplayOrder(displayOrder);
        
        StartDate = startDate;
        EndDate = endDate;
        
        if (userId != null)
            SetCreatedBy(userId);
    }
    
    // Construtor para criar um ProductTag apenas com IDs
    public ProductTag(Guid productId, Guid tagId, bool isFeatured = false, int displayOrder = 0, DateTime? startDate = null, DateTime? endDate = null, string? userId = null)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));
            
        if (tagId == Guid.Empty)
            throw new ArgumentException("Tag ID cannot be empty", nameof(tagId));
            
        ProductId = productId;
        TagId = tagId;
        IsFeatured = isFeatured;
        SetDisplayOrder(displayOrder);
        
        StartDate = startDate;
        EndDate = endDate;
        
        if (userId != null)
            SetCreatedBy(userId);
    }
    
    // Métodos de negócio
    public void Update(bool? isFeatured = null, int? displayOrder = null, DateTime? startDate = null, DateTime? endDate = null, string? userId = null)
    {
        if (isFeatured.HasValue)
            IsFeatured = isFeatured.Value;
            
        if (displayOrder.HasValue)
            SetDisplayOrder(displayOrder.Value);
            
        if (startDate.HasValue)
            StartDate = startDate.Value;
            
        if (endDate.HasValue)
            EndDate = endDate.Value;
            
        if (userId != null)
            UpdateAuditFields(userId);
    }
    
    public void SetFeatured(bool isFeatured, string? userId = null)
    {
        IsFeatured = isFeatured;
        
        if (userId != null)
            UpdateAuditFields(userId);
    }
    
    public void SetDisplayOrder(int order, string? userId = null)
    {
        if (order < 0)
            throw new DomainException("A ordem de exibição não pode ser negativa");
            
        DisplayOrder = order;
        
        if (userId != null)
            UpdateAuditFields(userId);
    }
    
    public void SetDateRange(DateTime? startDate, DateTime? endDate, string? userId = null)
    {
        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            throw new DomainException("A data de início não pode ser posterior à data de término");
            
        StartDate = startDate;
        EndDate = endDate;
        
        if (userId != null)
            UpdateAuditFields(userId);
    }
    
    public bool IsActive()
    {
        var now = DateTime.UtcNow;
        bool isActive = true;
        
        if (StartDate.HasValue && StartDate > now)
            isActive = false;
            
        if (EndDate.HasValue && EndDate < now)
            isActive = false;
            
        return isActive;
    }
}
