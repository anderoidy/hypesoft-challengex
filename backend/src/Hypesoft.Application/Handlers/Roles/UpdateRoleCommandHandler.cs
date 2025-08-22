using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Hypesoft.Application.Commands.Roles;
using Hypesoft.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Application.Handlers.Roles
{
    public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, Result<Guid>>
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger<UpdateRoleCommandHandler> _logger;

        public UpdateRoleCommandHandler(
            RoleManager<ApplicationRole> roleManager,
            ILogger<UpdateRoleCommandHandler> logger
        )
        {
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Guid>> Handle(
            UpdateRoleCommand request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                _logger.LogInformation("Updating role with ID {RoleId}", request.Id);

                // Find the role by ID
                var role = await _roleManager.FindByIdAsync(request.Id.ToString());
                if (role == null)
                {
                    _logger.LogWarning("Role with ID {RoleId} not found", request.Id);
                    return Result<Guid>.NotFound($"Role with ID {request.Id} not found");
                }

                // Check if the role is a system role (prevent modification of system roles)
                if (role.IsSystemRole)
                {
                    _logger.LogWarning("Cannot modify system role {RoleName}", role.Name);
                    return Result<Guid>.Error("System roles cannot be modified");
                }

                // Check if a role with the new name already exists (if name is being changed)
                if (!string.Equals(role.Name, request.Name, StringComparison.OrdinalIgnoreCase))
                {
                    var roleExists = await _roleManager.RoleExistsAsync(request.Name);
                    if (roleExists)
                    {
                        _logger.LogWarning(
                            "A role with name {RoleName} already exists",
                            request.Name
                        );
                        return Result<Guid>.Conflict(
                            $"A role with name '{request.Name}' already exists"
                        );
                    }
                }

                // Update role properties
                role.Name = request.Name;
                role.Description = request.Description;
                role.SetLastModifiedBy(request.ModifiedBy ?? "System");

                // Save changes
                var result = await _roleManager.UpdateAsync(role);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError(
                        "Failed to update role {RoleName}: {Errors}",
                        role.Name,
                        errors
                    );
                    return Result<Guid>.Error($"Failed to update role: {errors}");
                }

                // Converter ID para Guid se necessário
                if (Guid.TryParse(role.Id.ToString(), out var roleGuid))
                {
                    _logger.LogInformation(
                        "Successfully updated role {RoleName} with ID {RoleId}",
                        role.Name,
                        roleGuid
                    );
                    return Result<Guid>.Success(roleGuid);
                }
                else
                {
                    _logger.LogError("Failed to parse role ID {RoleId} as Guid", role.Id);
                    return Result<Guid>.Error("Failed to parse role ID");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating role with ID {RoleId}: {ErrorMessage}",
                    request.Id,
                    ex.Message
                );
                return Result<Guid>.Error(
                    $"An error occurred while updating the role: {ex.Message}"
                );
            }
        }
    }
}
