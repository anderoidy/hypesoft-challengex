using Ardalis.Result;
using MediatR;

namespace Hypesoft.Application.Commands.Products
{
    /// <summary>
    /// Command CQRS para exclusão de um produto.
    /// </summary>
    public record DeleteProductCommand(Guid Id, string? DeletedBy = null) : IRequest<Result>; // ← MUDANÇA: Result simples ao invés de Result<bool>
}
