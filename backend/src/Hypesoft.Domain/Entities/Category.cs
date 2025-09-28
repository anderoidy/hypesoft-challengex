using System;
using System.Collections.Generic;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Exceptions;
using Hypesoft.Domain.Interfaces;

namespace Hypesoft.Domain.Entities;

public class Category : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }
    public string Slug { get; private set; } = string.Empty;

    // Navigation properties
    public Guid? ParentCategoryId { get; private set; }
    public virtual Category? ParentCategory { get; private set; }
    public virtual ICollection<Category> ChildCategories { get; private set; } =
        new List<Category>();
    public virtual ICollection<Category> SubCategories { get; private set; } = new List<Category>();
    public virtual ICollection<Product> Products { get; private set; } = new List<Product>();

    // 🔹 Construtor protegido para EF/Mongo
    protected Category()
        : base("system") { }

    // Dentro de Category (que herda BaseEntity)
    public void EnsureId()
    {
        if (Id == Guid.Empty)
            SetId(Guid.NewGuid());
    }

    // 🔹 Construtor principal
    public Category(
        string name,
        string? description = null,
        string? imageUrl = null,
        Guid? parentCategoryId = null,
        string? slug = null,
        bool isActive = true,
        string? createdBy = null
    )
        : base(createdBy ?? "system")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("O nome da categoria não pode ser vazio", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        ImageUrl = imageUrl;
        ParentCategoryId = parentCategoryId;
        SetSlug(slug ?? GenerateSlug(name));

        if (!isActive)
            Deactivate(createdBy ?? "system");
    }

    public void Update(
        string name,
        string? description = null,
        string? imageUrl = null,
        string? slug = null,
        string? userId = null
    )
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("O nome da categoria não pode ser vazio", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();

        if (!string.IsNullOrEmpty(imageUrl))
            ImageUrl = imageUrl;

        if (!string.IsNullOrEmpty(slug))
            SetSlug(slug);

        UpdateAuditFields(userId ?? "system");
    }

    public void SetSlug(string slug)
    {
        Slug = string.IsNullOrWhiteSpace(slug) ? GenerateSlug(Name) : slug.Trim().ToLower();
        UpdateAuditFields("system");
    }

    private string GenerateSlug(string name)
    {
        if (string.IsNullOrEmpty(name))
            return string.Empty;

        return name.ToLower()
            .Replace(" ", "-")
            .Replace("&", "and")
            .Replace("#", "sharp")
            .Replace("+", "plus")
            .Replace("á", "a")
            .Replace("é", "e")
            .Replace("í", "i")
            .Replace("ó", "o")
            .Replace("ú", "u")
            .Replace("ã", "a")
            .Replace("õ", "o")
            .Replace("ç", "c");
    }

    public void ChangeParent(Guid? newParentId, string? userId = null)
    {
        if (ParentCategoryId == newParentId)
            return;

        ParentCategoryId = newParentId;
        UpdateAuditFields(userId ?? "system");
    }

    public void AddSubCategory(Category subCategory, string? userId = null)
    {
        if (subCategory == null)
            throw new ArgumentNullException(nameof(subCategory));

        if (subCategory.Id == Id)
            throw new InvalidOperationException(
                "Uma categoria não pode ser subcategoria de si mesma"
            );

        if (subCategory.ParentCategoryId == Id)
            return; // já é subcategoria desta categoria

        subCategory.ParentCategory?.RemoveSubCategory(subCategory, userId);

        subCategory.ParentCategoryId = Id;
        subCategory.ParentCategory = this;
        subCategory.Deactivate(userId ?? "system");

        SubCategories.Add(subCategory);
    }

    public void RemoveSubCategory(Category subCategory, string? userId = null)
    {
        if (subCategory == null)
            throw new ArgumentNullException(nameof(subCategory));

        if (subCategory.ParentCategoryId != Id)
            return;

        subCategory.ParentCategoryId = null;
        subCategory.ParentCategory = null;
        subCategory.Activate(userId ?? "system");

        SubCategories.Remove(subCategory);
    }

    public void SetAsMainCategory(bool isMain, string? userId = null)
    {
        if (IsActive == isMain)
            return;

        if (!isMain && ParentCategoryId == null)
            throw new InvalidOperationException(
                "Uma categoria raiz deve ser uma categoria principal"
            );

        if (isMain)
            Activate(userId ?? "system");
        else
            Deactivate(userId ?? "system");
    }

    public void UpdateImage(string imageUrl, string? userId = null)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("A URL da imagem não pode ser vazia", nameof(imageUrl));

        ImageUrl = imageUrl;
        UpdateAuditFields(userId ?? "system");
    }
}
