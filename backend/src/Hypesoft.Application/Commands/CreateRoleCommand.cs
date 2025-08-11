using Ardalis.Result;
using Hypesoft.Domain.Entities;
using MediatR;

namespace Hypesoft.Application.Commands
{
    /// <summary>
    /// Command CQRS para criação de uma nova função (role).
    /// </summary>
    public record CreateRoleCommand(
        string Name,
        string? Description = null,
        string? CreatedBy = null
    ) : IRequest<Result<ApplicationRole>>;
}
