using Hypesoft.Domain.Common;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Interfaces;

namespace Hypesoft.Domain.Entities;

public class Category : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }
    public bool IsMainCategory { get; private set; }
    public string? Slug { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    // Navigation properties
    public Guid? ParentCategoryId { get; private set; }
    public virtual Category? ParentCategory { get; private set; }
    public virtual ICollection<Category> ChildCategories { get; private set; } = new List<Category>();
    public virtual ICollection<Category> SubCategories { get; private set; } = new List<Category>();
    public virtual ICollection<Product> Products { get; private set; } = new List<Product>();

    // Método para atualizar a data de atualização
    public void SetUpdatedAt(DateTime updatedAt)
    {
        UpdatedAt = updatedAt;
    }

    protected Category() { } // Private constructor for EF Core

    public Category(string name, string? description = null, string? imageUrl = null, bool isMainCategory = true, Guid? parentCategoryId = null, string? slug = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("O nome da categoria não pode ser vazio", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        ImageUrl = imageUrl;
        IsMainCategory = isMainCategory;
        ParentCategoryId = parentCategoryId;
        SetSlug(slug ?? GenerateSlug(name));
    }

    // Adicionando o método UpdateAuditFields para atualizar os campos de auditoria
    public void UpdateAuditFields(string userId)
    {
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string? description = null, string? imageUrl = null, string? userId = null, string? slug = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("O nome da categoria não pode ser vazio", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        
        if (!string.IsNullOrEmpty(imageUrl))
            ImageUrl = imageUrl;
            
        if (!string.IsNullOrEmpty(slug))
            SetSlug(slug);

        if (!string.IsNullOrEmpty(userId))
            UpdateAuditFields(userId);
            
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void SetSlug(string slug)
    {
        Slug = string.IsNullOrWhiteSpace(slug) ? GenerateSlug(Name) : slug.Trim().ToLower();
        UpdatedAt = DateTime.UtcNow;
    }
    
    private string GenerateSlug(string name)
    {
        if (string.IsNullOrEmpty(name))
            return string.Empty;
            
        return name.ToLower()
                 .Replace(" ", "-")
                 .Replace("&", "and")
                 .Replace("#", "sharp")
                 .Replace("+", "plus");
    }

    public void AddSubCategory(Category subCategory, string? userId = null)
    {
        if (subCategory == null)
            throw new ArgumentNullException(nameof(subCategory));

        if (subCategory.Id == Id)
            throw new InvalidOperationException("Uma categoria não pode ser subcategoria de si mesma");

        if (subCategory.ParentCategoryId == Id)
            return; // Já é uma subcategoria desta categoria

        // Remove de outra categoria pai, se houver
        if (subCategory.ParentCategory != null)
            subCategory.ParentCategory.RemoveSubCategory(subCategory);

        subCategory.ParentCategoryId = Id;
        subCategory.ParentCategory = this;
        subCategory.IsMainCategory = false;
        
        if (!string.IsNullOrEmpty(userId))
            subCategory.UpdateAuditFields(userId);
            
        SubCategories.Add(subCategory);
    }

    public void RemoveSubCategory(Category subCategory, string? userId = null)
    {
        if (subCategory == null)
            throw new ArgumentNullException(nameof(subCategory));

        if (subCategory.ParentCategoryId != Id)
            return; // Não é subcategoria desta categoria

        subCategory.ParentCategoryId = null;
        subCategory.ParentCategory = null;
        subCategory.IsMainCategory = true;
        
        if (!string.IsNullOrEmpty(userId))
            subCategory.UpdateAuditFields(userId);
            
        SubCategories.Remove(subCategory);
    }

    public void SetAsMainCategory(bool isMain, string? userId = null)
    {
        if (IsMainCategory == isMain)
            return;

        if (!isMain && ParentCategoryId == null)
            throw new InvalidOperationException("Uma categoria raiz deve ser uma categoria principal");

        IsMainCategory = isMain;
        
        if (!string.IsNullOrEmpty(userId))
            UpdateAuditFields(userId);
    }

    public void UpdateImage(string imageUrl, string? userId = null)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("A URL da imagem não pode ser vazia", nameof(imageUrl));

        ImageUrl = imageUrl;
        
        if (!string.IsNullOrEmpty(userId))
            UpdateAuditFields(userId);
    }
}
