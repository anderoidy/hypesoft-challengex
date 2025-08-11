using Ardalis.Result;
using Hypesoft.Domain.Entities;
using MediatR;

namespace Hypesoft.Application.Queries
{
    /// <summary>
    /// Query CQRS para obter uma tag pelo ID.
    /// </summary>
    public record GetTagByIdQuery(
        Guid Id
    ) : IRequest<Result<Tag>>;
}
