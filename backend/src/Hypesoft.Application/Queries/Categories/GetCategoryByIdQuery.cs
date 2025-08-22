using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using Hypesoft.Application.DTOs;
using MediatR;

namespace Hypesoft.Application.Queries.Categories
{
    /// <summary>
    /// Query CQRS para obter categoria por ID.
    /// </summary>
    public record GetCategoryByIdQuery([Required] Guid Id) : IRequest<Result<CategoryDto>>;
}
