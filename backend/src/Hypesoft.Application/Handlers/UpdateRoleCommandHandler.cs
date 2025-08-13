using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Hypesoft.Application.Commands;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Hypesoft.Application.Handlers
{
    public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, Result<ApplicationRole>>
    {
        private readonly IRoleRepository _roleRepository;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public UpdateRoleCommandHandler(
            IRoleRepository roleRepository,
            RoleManager<ApplicationRole> roleManager)
        {
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        public async Task<Result<ApplicationRole>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Busca a função existente
                var existingRole = await _roleManager.FindByIdAsync(request.Id.ToString());
                if (existingRole == null)
                {
                    return Result<ApplicationRole>.NotFound($"Função com ID {request.Id} não encontrada.");
                }

                // Verifica se já existe outra função com o mesmo nome
                var roleWithSameName = await _roleManager.FindByNameAsync(request.Name);
                if (roleWithSameName != null && roleWithSameName.Id != request.Id)
                {
                    return Result<ApplicationRole>.Error($"Já existe uma função com o nome '{request.Name}'.");
                }

                // Atualiza os dados da função
                existingRole.Name = request.Name;
                existingRole.NormalizedName = request.Name.ToUpperInvariant();
                existingRole.Description = request.Description;
                
                // Atualiza informações de auditoria usando os métodos apropriados
                existingRole.SetLastModifiedBy(request.ModifiedBy ?? "System");

                // Salva as alterações
                var result = await _roleManager.UpdateAsync(existingRole);
                
                if (!result.Succeeded)
                {
                    return Result<ApplicationRole>.Error("Falha ao atualizar a função: " + string.Join(", ", result.Errors));
                }

                return Result<ApplicationRole>.Success(existingRole);
            }
            catch (Exception ex)
            {
                return Result<ApplicationRole>.Error($"Erro ao atualizar a função: {ex.Message}");
            }
        }
    }
}
