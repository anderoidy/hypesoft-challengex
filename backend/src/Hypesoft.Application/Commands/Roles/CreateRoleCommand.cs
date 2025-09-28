// Commands/Roles/CreateRoleCommand.cs
using Ardalis.Result;
using MediatR;

namespace Hypesoft.Application.Commands.Roles;

public record CreateRoleCommand(string Name, string? Description = null) : IRequest<Result<Guid>>;
