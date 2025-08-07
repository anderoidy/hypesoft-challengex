using Ardalis.Result;
using MediatR;
using Hypesoft.Application.DTOs;

namespace Hypesoft.Application.Queries;

public record GetProductByIdQuery(Guid Id) : IRequest<Result<ProductDto>>;
