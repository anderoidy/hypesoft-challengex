using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Hypesoft.Application.Commands;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hypesoft.Application.Handlers
{
    public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Result>
    {
        private readonly IRoleRepository _roleRepository;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public DeleteRoleCommandHandler(
            IRoleRepository roleRepository,
            RoleManager<ApplicationRole> roleManager)
        {
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Busca a função existente
                var existingRole = await _roleManager.FindByIdAsync(request.Id.ToString());
                if (existingRole == null)
                {
                    return Result.NotFound($"Função com ID {request.Id} não encontrada.");
                }

                // Verifica se existem usuários associados a esta função
                var usersInRole = await _roleManager.Users
                    .Where(u => u.Roles.Any(r => r.RoleId == request.Id))
                    .CountAsync(cancellationToken);

                if (usersInRole > 0)
                {
                    return Result.Error("Não é possível excluir a função pois existem usuários associados a ela.");
                }

                // Remove a função
                var result = await _roleManager.DeleteAsync(existingRole);
                
                if (!result.Succeeded)
                {
                    return Result.Error("Falha ao excluir a função: " + string.Join(", ", result.Errors));
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Error($"Erro ao excluir a função: {ex.Message}");
            }
        }
    }
}
