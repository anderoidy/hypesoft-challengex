namespace Hypesoft.Application.DTOs;

public record CategoryDto(
    Guid Id,
    string Name,
    string? Description = null,
    string? ImageUrl = null,
    bool IsMainCategory = true,
    Guid? ParentCategoryId = null,
    string? ParentCategoryName = null)
{
    // Propriedade de navegação para subcategorias (opcional, dependendo da necessidade)
    public ICollection<CategoryDto>? SubCategories { get; set; }
}
