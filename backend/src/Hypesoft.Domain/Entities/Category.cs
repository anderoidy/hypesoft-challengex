using Hypesoft.Domain.Common;

namespace Hypesoft.Domain.Entities;

public class Category : EntityBase
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }
    public bool IsMainCategory { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public virtual Category? ParentCategory { get; private set; }
    public virtual ICollection<Category> SubCategories { get; private set; } = new List<Category>();
    public virtual ICollection<Product> Products { get; private set; } = new List<Product>();

    protected Category() { } // Construtor privado para o EF Core

    public Category(string name, string? description = null, string? imageUrl = null, bool isMainCategory = true, Guid? parentCategoryId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("O nome da categoria não pode ser vazio", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        ImageUrl = imageUrl;
        IsMainCategory = isMainCategory;
        ParentCategoryId = parentCategoryId;
    }

    public void Update(string name, string? description = null, string? imageUrl = null, string? userId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("O nome da categoria não pode ser vazio", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        
        if (!string.IsNullOrEmpty(imageUrl))
            ImageUrl = imageUrl;

        if (userId != null)
            UpdateAuditFields(userId);
    }

    public void AddSubCategory(Category subCategory, string? userId = null)
    {
        if (subCategory == null)
            throw new ArgumentNullException(nameof(subCategory));

        if (subCategory.Id == Id)
            throw new InvalidOperationException("Uma categoria não pode ser subcategoria de si mesma");

        if (subCategory.ParentCategoryId == Id)
            return; // Já é uma subcategoria desta categoria

        subCategory.ParentCategoryId = Id;
        subCategory.IsMainCategory = false;
        SubCategories.Add(subCategory);

        if (userId != null)
        {
            UpdateAuditFields(userId);
            subCategory.UpdateAuditFields(userId);
        }
    }

    public void RemoveSubCategory(Guid subCategoryId, string? userId = null)
    {
        var subCategory = SubCategories.FirstOrDefault(sc => sc.Id == subCategoryId);
        if (subCategory != null)
        {
            SubCategories.Remove(subCategory);
            subCategory.ParentCategoryId = null;
            subCategory.IsMainCategory = true;

            if (userId != null)
            {
                UpdateAuditFields(userId);
                subCategory.UpdateAuditFields(userId);
            }
        }
    }

    public void SetAsMainCategory(string? userId = null)
    {
        if (IsMainCategory)
            return;

        IsMainCategory = true;
        ParentCategoryId = null;
        ParentCategory = null;

        if (userId != null)
            UpdateAuditFields(userId);
    }
}
