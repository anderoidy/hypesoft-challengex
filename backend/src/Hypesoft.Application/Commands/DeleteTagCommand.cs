using Ardalis.Result;
using MediatR;

namespace Hypesoft.Application.Commands
{
    /// <summary>
    /// Command CQRS para exclusão de uma Tag.
    /// </summary>
    public record DeleteTagCommand(
        Guid Id,
        string? UserId = null
    ) : IRequest<Result>;
}
