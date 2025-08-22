using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Hypesoft.Application.Commands.Roles;
using Hypesoft.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Application.Handlers.Roles
{
    public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Result>
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DeleteRoleCommandHandler> _logger;

        public DeleteRoleCommandHandler(
            RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ILogger<DeleteRoleCommandHandler> logger)
        {
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting role with ID {RoleId}", request.Id);

                // Find the role by ID
                var role = await _roleManager.FindByIdAsync(request.Id.ToString());
                if (role == null)
                {
                    _logger.LogWarning("Role with ID {RoleId} not found", request.Id);
                    return Result.NotFound($"Role with ID {request.Id} not found");
                }

                // Check if role is a system role (prevent deletion of system roles)
                if (role.IsSystemRole)
                {
                    _logger.LogWarning("Cannot delete system role {RoleName}", role.Name);
                    return Result.Error("System roles cannot be deleted");
                }

                // Check if any users are assigned to this role
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
                if (usersInRole.Any())
                {
                    _logger.LogWarning("Cannot delete role {RoleName} because it has {UserCount} users assigned", 
                        role.Name, usersInRole.Count);
                    return Result.Error("Cannot delete role because it has users assigned. Please reassign or remove users first.");
                }

                // Delete the role
                var result = await _roleManager.DeleteAsync(role);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to delete role {RoleName}: {Errors}", role.Name, errors);
                    return Result.Error($"Failed to delete role: {errors}");
                }

                _logger.LogInformation("Successfully deleted role {RoleName}", role.Name);
                return Result.Success();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while deleting role with ID {RoleId}", request.Id);
                return Result.Error("A database error occurred while deleting the role. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role with ID {RoleId}: {ErrorMessage}", 
                    request.Id, ex.Message);
                return Result.Error($"An unexpected error occurred while deleting the role: {ex.Message}");
            }
        }
    }
}
