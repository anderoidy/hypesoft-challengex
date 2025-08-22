using Ardalis.Result;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Hypesoft.Application.Commands.Categories
{
    /// <summary>
    /// Command CQRS para criação de uma nova Categoria.
    /// </summary>
    public sealed record CreateCategoryCommand : IRequest<Result<Guid>>
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; init; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; init; }

        public bool IsActive { get; init; } = true;

        public string? CreatedBy { get; init; }

        public CreateCategoryCommand(
            string name,
            string? description = null,
            bool isActive = true,
            string? createdBy = null)
        {
            Name = name;
            Description = description;
            IsActive = isActive;
            CreatedBy = createdBy;
        }
    }
}
