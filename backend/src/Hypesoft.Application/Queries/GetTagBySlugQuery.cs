using Ardalis.Result;
using Hypesoft.Domain.Entities;
using MediatR;

namespace Hypesoft.Application.Queries
{
    /// <summary>
    /// Query CQRS para obter uma tag pelo slug.
    /// </summary>
    public record GetTagBySlugQuery(
        string Slug
    ) : IRequest<Result<Tag>>;
}
