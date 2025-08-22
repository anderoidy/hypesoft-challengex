using Ardalis.Result;
using MediatR;

namespace Hypesoft.Application.Commands.Roles
{
    /// <summary>
    /// Command CQRS para exclusão de uma função (role) existente.
    /// </summary>
    public record DeleteRoleCommand(Guid Id) : IRequest<Result>;
}
