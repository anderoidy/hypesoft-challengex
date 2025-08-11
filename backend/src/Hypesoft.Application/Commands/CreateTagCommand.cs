using Ardalis.Result;
using MediatR;

namespace Hypesoft.Application.Commands
{
    /// <summary>
    /// Command CQRS para criação de uma nova Tag.
    /// </summary>
    public record CreateTagCommand(
        string Name,
        string? Description = null,
        string? Icon = null,
        string? Color = null,
        bool IsActive = true,
        int DisplayOrder = 0,
        string? UserId = null
    ) : IRequest<Result<Tag>>;
}
