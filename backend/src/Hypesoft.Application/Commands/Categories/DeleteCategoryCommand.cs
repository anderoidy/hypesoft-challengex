using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using MediatR;

namespace Hypesoft.Application.Commands.Categories
{
    /// <summary>
    /// Command CQRS para deleção de uma Categoria.
    /// </summary>
    public sealed record DeleteCategoryCommand(
        [Required(ErrorMessage = "Category ID is required")] Guid Id
    ) : IRequest<Result<bool>>;
}
