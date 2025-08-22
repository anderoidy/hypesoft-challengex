using Ardalis.Result;
using MediatR;
using Hypesoft.Application.DTOs;

namespace Hypesoft.Application.Queries.Products
{
public record GetProductByIdQuery(Guid Id) : IRequest<Result<ProductDto>>;
}