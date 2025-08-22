using MediatR;
using Ardalis.Result;
using System.ComponentModel.DataAnnotations;

namespace Hypesoft.Application.Commands.Products
{
    /// <summary>
    /// Command CQRS para deleção de um Produto.
    /// </summary>
    public sealed record DeleteProductCommand(
        [Required(ErrorMessage = "Product ID is required")] Guid Id
    ) : IRequest<Result<bool>>;
}
