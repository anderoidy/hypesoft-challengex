using Hypesoft.Domain.Common;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Exceptions;

namespace Hypesoft.Domain.Entities;

public class Tag : EntityBase, INamedEntity
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string? Icon { get; private set; }
    public string? Color { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int DisplayOrder { get; private set; }
    
    // Relacionamentos
    public virtual ICollection<ProductTag> ProductTags { get; private set; } = new List<ProductTag>();
    
    // Construtor privado para o EF Core
    protected Tag() { }
    
    public Tag(
        string name,
        string? description = null,
        string? icon = null,
        string? color = null,
        bool isActive = true,
        int displayOrder = 0,
        string? userId = null)
    {
        SetName(name);
        Description = description?.Trim();
        Icon = icon;
        SetColor(color);
        IsActive = isActive;
        SetDisplayOrder(displayOrder);
        
        if (userId != null)
            SetCreatedBy(userId);
    }
    
    // Métodos de negócio
    public void Update(
        string name,
        string? description = null,
        string? icon = null,
        string? color = null,
        bool? isActive = null,
        int? displayOrder = null,
        string? userId = null)
    {
        SetName(name);
        
        if (description != null)
            Description = description.Trim();
            
        if (icon != null)
            Icon = icon;
            
        if (color != null)
            SetColor(color);
            
        if (isActive.HasValue)
            IsActive = isActive.Value;
            
        if (displayOrder.HasValue)
            SetDisplayOrder(displayOrder.Value);
            
        if (userId != null)
            UpdateAuditFields(userId);
    }
    
    public void Activate(string? userId = null)
    {
        if (IsActive)
            return;
            
        IsActive = true;
        
        if (userId != null)
            UpdateAuditFields(userId);
    }
    
    public void Deactivate(string? userId = null)
    {
        if (!IsActive)
            return;
            
        IsActive = false;
        
        if (userId != null)
            UpdateAuditFields(userId);
    }
    
    // Métodos privados para validação
    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome da tag não pode ser vazio");
            
        if (name.Length > 100)
            throw new DomainException("O nome da tag não pode ter mais de 100 caracteres");
            
        Name = name.Trim();
    }
    
    private void SetColor(string? color)
    {
        if (!string.IsNullOrEmpty(color) && color.Length > 20)
            throw new DomainException("O código de cor não pode ter mais de 20 caracteres");
            
        Color = color;
    }
    
    private void SetDisplayOrder(int order)
    {
        if (order < 0)
            throw new DomainException("A ordem de exibição não pode ser negativa");
            
        DisplayOrder = order;
    }
}
