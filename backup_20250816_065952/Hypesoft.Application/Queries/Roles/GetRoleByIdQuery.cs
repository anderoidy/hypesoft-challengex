using Ardalis.Result;
using Hypesoft.Application.DTOs;
using MediatR;

namespace Hypesoft.Application.Queries.Roles
{
    /// <summary>
    /// Query to get a single role by its ID
    /// </summary>
    /// <param name="Id">The unique identifier of the role</param>
    public record GetRoleByIdQuery(Guid Id) : IRequest<Result<RoleDto>>;
}
