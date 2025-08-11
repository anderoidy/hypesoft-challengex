using Ardalis.Result;
using Hypesoft.Domain.Entities;
using MediatR;

namespace Hypesoft.Application.Commands
{
    /// <summary>
    /// Command CQRS para atualização de uma função (role) existente.
    /// </summary>
    public record UpdateRoleCommand(
        Guid Id,
        string Name,
        string? Description = null,
        string? ModifiedBy = null
    ) : IRequest<Result<ApplicationRole>>;
}
