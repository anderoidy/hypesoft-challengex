using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Hypesoft.Application.Features.UserSessions.Commands;
using Hypesoft.Application.Features.UserSessions.Queries;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Hypesoft.Infrastructure.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hypesoft.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserSessionsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly ILogger<UserSessionsController> _logger;

        public UserSessionsController(
            IMediator mediator,
            IUserSessionRepository userSessionRepository,
            ILogger<UserSessionsController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _userSessionRepository = userSessionRepository ?? throw new ArgumentNullException(nameof(userSessionRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtém todas as sessões ativas do usuário atual
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Lista de sessões do usuário</returns>
        [HttpGet("my-sessions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyList<UserSession>>> GetMySessions(CancellationToken cancellationToken)
        {
            try
            {
                var userId = User.GetUserId();
                var sessions = await _userSessionRepository.GetUserSessionsAsync(userId, cancellationToken);
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter sessões do usuário");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar sua solicitação");
            }
        }

        /// <summary>
        /// Obtém todas as sessões ativas (apenas para administradores)
        /// </summary>
        /// <param name="pageNumber">Número da página (padrão: 1)</param>
        /// <param name="pageSize">Itens por página (padrão: 20, máximo: 100)</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Lista paginada de sessões ativas</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedList<UserSession>>> GetAllSessions(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            try
            {
                pageNumber = Math.Max(1, pageNumber);
                pageSize = Math.Clamp(pageSize, 1, 100);

                var result = await _userSessionRepository.GetPaginatedSessionsAsync(
                    pageNumber,
                    pageSize,
                    cancellationToken);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter sessões ativas");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar sua solicitação");
            }
        }

        /// <summary>
        /// Obtém uma sessão pelo ID
        /// </summary>
        /// <param name="id">ID da sessão</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Dados da sessão</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserSession>> GetSessionById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var session = await _userSessionRepository.GetByIdWithDetailsAsync(id, cancellationToken);
                if (session == null)
                {
                    return NotFound();
                }

                // Usuário só pode ver a própria sessão, a menos que seja admin
                var userId = User.GetUserId();
                if (session.UserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                return Ok(session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter sessão com ID {SessionId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar sua solicitação");
            }
        }

        /// <summary>
        /// Encerra a sessão atual do usuário
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Resposta sem conteúdo</returns>
        [HttpPost("end-current")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EndCurrentSession(CancellationToken cancellationToken)
        {
            try
            {
                var userId = User.GetUserId();
                var sessionId = User.GetSessionId();

                if (string.IsNullOrEmpty(sessionId))
                {
                    return BadRequest("Nenhuma sessão ativa encontrada");
                }

                var command = new EndSessionCommand { SessionId = sessionId, UserId = userId };
                var result = await _mediator.Send(command, cancellationToken);

                if (result.Status == ResultStatus.NotFound)
                {
                    return NotFound("Sessão não encontrada");
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
                _logger.LogError(ex, "Erro ao encerrar sessão atual");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar sua solicitação");
            }
        }

        /// <summary>
        /// Encerra uma sessão específica
        /// </summary>
        /// <param name="id">ID da sessão</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Resposta sem conteúdo</returns>
        [HttpPost("{id:guid}/end")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EndSession(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var userId = User.GetUserId();
                var isAdmin = User.IsInRole("Admin");
                
                var session = await _userSessionRepository.GetByIdAsync(id, cancellationToken);
                if (session == null)
                {
                    return NotFound("Sessão não encontrada");
                }

                // Usuário só pode encerrar a própria sessão, a menos que seja admin
                if (session.UserId != userId && !isAdmin)
                {
                    return Forbid();
                }

                var command = new EndSessionCommand { SessionId = id.ToString(), UserId = userId, Force = isAdmin };
                var result = await _mediator.Send(command, cancellationToken);

                if (result.Status == ResultStatus.NotFound)
                {
                    return NotFound("Sessão não encontrada");
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
                _logger.LogError(ex, "Erro ao encerrar sessão com ID {SessionId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar sua solicitação");
            }
        }

        /// <summary>
        /// Encerra todas as sessões do usuário atual, exceto a atual
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Resposta sem conteúdo</returns>
        [HttpPost("end-others")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EndOtherSessions(CancellationToken cancellationToken)
        {
            try
            {
                var userId = User.GetUserId();
                var currentSessionId = User.GetSessionId();

                if (string.IsNullOrEmpty(currentSessionId))
                {
                    return BadRequest("Nenhuma sessão ativa encontrada");
                }

                var command = new EndOtherSessionsCommand { UserId = userId, CurrentSessionId = currentSessionId };
                var result = await _mediator.Send(command, cancellationToken);

                if (result.Status == ResultStatus.Invalid)
                {
                    result.ValidationErrors.ForEach(error => ModelState.AddModelError(error.Identifier, error.ErrorMessage));
                    return ValidationProblem(ModelState);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao encerrar outras sessões do usuário");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar sua solicitação");
            }
        }

        /// <summary>
        /// Encerra todas as sessões de um usuário (apenas para administradores)
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Resposta sem conteúdo</returns>
        [HttpPost("end-all/{userId:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EndAllUserSessions(Guid userId, CancellationToken cancellationToken)
        {
            try
            {
                var command = new EndAllUserSessionsCommand { UserId = userId };
                var result = await _mediator.Send(command, cancellationToken);

                if (result.Status == ResultStatus.NotFound)
                {
                    return NotFound("Usuário não encontrado");
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
                _logger.LogError(ex, "Erro ao encerrar todas as sessões do usuário {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar sua solicitação");
            }
        }
    }
}
