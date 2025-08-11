using Ardalis.Result;
using Hypesoft.Domain.Entities;
using MediatR;

namespace Hypesoft.Application.Queries
{
    /// <summary>
    /// Query CQRS para obter todas as tags com suporte a paginação.
    /// </summary>
    public record GetAllTagsQuery(
        int PageNumber = 1,
        int PageSize = 20,
        string? SearchTerm = null,
        bool? IsActive = null,
        string? SortBy = null,
        bool SortDescending = false
    ) : IRequest<Result<PaginatedList<Tag>>>;
}
