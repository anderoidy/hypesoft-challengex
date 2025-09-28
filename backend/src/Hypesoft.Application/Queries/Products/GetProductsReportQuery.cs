using Ardalis.Result;
using Hypesoft.Application.DTOs;
using MediatR;

namespace Hypesoft.Application.Queries.Products
{
    public record GetProductsReportQuery() : IRequest<Result<ProductsReportDto>>;
}
