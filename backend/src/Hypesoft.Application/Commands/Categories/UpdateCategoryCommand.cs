using Ardalis.Result;
using MediatR;
using System.ComponentModel.DataAnnotations;
using Hypesoft.Application.DTOs;

namespace Hypesoft.Application.Commands.Categories
{
    /// <summary>
    /// Command CQRS para atualização de uma Categoria.
    /// </summary>
    public sealed record UpdateCategoryCommand : IRequest<Result<CategoryDto>>
    {
        [Required(ErrorMessage = "Category ID is required")]
        public Guid Id { get; init; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; init; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; init; }

        public bool IsActive { get; init; } = true;

        public string? ModifiedBy { get; init; }

        public UpdateCategoryCommand(
            Guid id,
            string name,
            string? description = null,
            bool isActive = true,
            string? modifiedBy = null)
        {
            Id = id;
            Name = name;
            Description = description;
            IsActive = isActive;
            ModifiedBy = modifiedBy;
        }
    }
}
