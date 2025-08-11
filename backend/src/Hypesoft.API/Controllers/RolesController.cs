using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Hypesoft.Application.Features.Roles.Commands;
using Hypesoft.Application.Features.Roles.Queries;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Hypesoft.Infrastructure.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hypesoft.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class RolesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IRoleRepository _roleRepository;
        private readonly ILogger<RolesController> _logger;

        public RolesController(
            IMediator mediator,
            IRoleRepository roleRepository,
            ILogger<RolesController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtém todas as funções do sistema
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Lista de funções</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyList<ApplicationRole>>> GetRoles(CancellationToken cancellationToken)
        {
            try
            {
                var roles = await _roleRepository.ListAllAsync(cancellationToken);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter funções");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar sua solicitação");
            }
        }

        /// <summary>
        /// Obtém uma função pelo ID
        /// </summary>
        /// <param name="id">ID da função</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Dados da função</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApplicationRole>> GetRoleById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var role = await _roleRepository.GetByIdWithDetailsAsync(id, cancellationToken);
                if (role == null)
                {
                    return NotFound();
                }

                return Ok(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter função com ID {RoleId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar sua solicitação");
            }
        }

        /// <summary>
        /// Obtém uma função pelo nome
        /// </summary>
        /// <param name="name">Nome da função</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Dados da função</returns>
        [HttpGet("byname/{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApplicationRole>> GetRoleByName(string name, CancellationToken cancellationToken)
        {
            try
            {
                var role = await _roleRepository.GetByNameAsync(name, cancellationToken);
                if (role == null)
                {
                    return NotFound();
                }

                return Ok(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter função com nome {RoleName}", name);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar sua solicitação");
            }
        }

        /// <summary>
        /// Cria uma nova função
        /// </summary>
        /// <param name="command">Dados da função</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Dados da função criada</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApplicationRole>> CreateRole([FromBody] CreateRoleCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _mediator.Send(command, cancellationToken);

                if (result.Status == ResultStatus.Invalid)
                {
                    result.ValidationErrors.ForEach(error => ModelState.AddModelError(error.Identifier, error.ErrorMessage));
                    return ValidationProblem(ModelState);
                }

                return CreatedAtAction(
                    nameof(GetRoleById), 
                    new { id = result.Value.Id }, 
                    result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar função");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar sua solicitação");
            }
        }

        /// <summary>
        /// Atualiza uma função existente
        /// </summary>
        /// <param name="id">ID da função</param>
        /// <param name="command">Dados atualizados da função</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Dados da função atualizada</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApplicationRole>> UpdateRole(
            Guid id, 
            [FromBody] UpdateRoleCommand command, 
            CancellationToken cancellationToken)
        {
            try
            {
                if (id != command.Id)
                {
                    return BadRequest("ID da rota não corresponde ao ID do comando");
                }

                var result = await _mediator.Send(command, cancellationToken);

                if (result.Status == ResultStatus.NotFound)
                {
                    return NotFound();
                }

                if (result.Status == ResultStatus.Invalid)
                {
                    result.ValidationErrors.ForEach(error => ModelState.AddModelError(error.Identifier, error.ErrorMessage));
                    return ValidationProblem(ModelState);
                }

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar função com ID {RoleId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar sua solicitação");
            }
        }

        /// <summary>
        /// Exclui uma função
        /// </summary>
        /// <param name="id">ID da função</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Resposta sem conteúdo</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteRole(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new DeleteRoleCommand { Id = id };
                var result = await _mediator.Send(command, cancellationToken);

                if (result.Status == ResultStatus.NotFound)
                {
                    return NotFound();
                }

                if (result.Status == ResultStatus.Invalid)
                {
                    result.ValidationErrors.ForEach(error => ModelState.AddModelError(error.Identifier, error.ErrorMessage));
                    return ValidationProblem(ModelState);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir função com ID {RoleId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar sua solicitação");
            }
        }

        /// <summary>
        /// Adiciona uma claim a uma função
        /// </summary>
        /// <param name="id">ID da função</param>
        /// <param name="command">Dados da claim</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Resposta sem conteúdo</returns>
        [HttpPost("{id:guid}/claims")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddClaimToRole(
            Guid id,
            [FromBody] AddClaimToRoleCommand command,
            CancellationToken cancellationToken)
        {
            try
            {
                if (id != command.RoleId)
                {
                    return BadRequest("ID da rota não corresponde ao ID da função no comando");
                }

                var result = await _mediator.Send(command, cancellationToken);

                if (result.Status == ResultStatus.NotFound)
                {
                    return NotFound("Função não encontrada");
                }

                if (result.Status == ResultStatus.Invalid)
                {
                    result.ValidationErrors.ForEach(error => ModelState.AddModelError(error.Identifier, error.ErrorMessage));
                    return ValidationProblem(ModelState);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar claim à função {RoleId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar sua solicitação");
            }
        }

        /// <summary>
        /// Remove uma claim de uma função
        /// </summary>
        /// <param name="id">ID da função</param>
        /// <param name="command">Dados da claim</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Resposta sem conteúdo</returns>
        [HttpDelete("{id:guid}/claims")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveClaimFromRole(
            Guid id,
            [FromBody] RemoveClaimFromRoleCommand command,
            CancellationToken cancellationToken)
        {
            try
            {
                if (id != command.RoleId)
                {
                    return BadRequest("ID da rota não corresponde ao ID da função no comando");
                }

                var result = await _mediator.Send(command, cancellationToken);

                if (result.Status == ResultStatus.NotFound)
                {
                    return NotFound("Função ou claim não encontrada");
                }

                if (result.Status == ResultStatus.Invalid)
                {
                    result.ValidationErrors.ForEach(error => ModelState.AddModelError(error.Identifier, error.ErrorMessage));
                    return ValidationProblem(ModelState);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover claim da função {RoleId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar sua solicitação");
            }
        }
    }
}
