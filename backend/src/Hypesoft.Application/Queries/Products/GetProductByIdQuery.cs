using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using Hypesoft.Application.DTOs;
using MediatR;

namespace Hypesoft.Application.Queries.Products
{
    /// <summary>
    /// Query CQRS para obter produto por ID.
    /// </summary>
    public record GetProductByIdQuery([Required] Guid Id) : IRequest<Result<ProductDto>>;
}
