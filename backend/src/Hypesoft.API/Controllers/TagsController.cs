using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Hypesoft.Application.Features.Tags.Commands;
using Hypesoft.Application.Features.Tags.Queries;
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
    public class TagsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ITagRepository _tagRepository;
        private readonly ILogger<TagsController> _logger;

        public TagsController(
            IMediator mediator,
            ITagRepository tagRepository,
            ILogger<TagsController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtém todas as tags com suporte a paginação
        /// </summary>
        /// <param name="pageNumber">Número da página (padrão: 1)</param>
        /// <param name="pageSize">Itens por página (padrão: 20, máximo: 100)</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Lista paginada de tags</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedList<Tag>>> GetTags(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            try
            {
                pageNumber = Math.Max(1, pageNumber);
                pageSize = Math.Clamp(pageSize, 1, 100);

                var result = await _tagRepository.GetPaginatedTagsAsync(
                    pageNumber, 
                    pageSize, 
                    cancellationToken);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter tags");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar sua solicitação");
            }
        }

        /// <summary>
        /// Obtém uma tag pelo ID
        /// </summary>
        /// <param name="id">ID da tag</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Dados da tag</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Tag>> GetTagById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var tag = await _tagRepository.GetByIdWithDetailsAsync(id, cancellationToken);
                if (tag == null)
                {
                    return NotFound();
                }

                return Ok(tag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter tag com ID {TagId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar sua solicitação");
            }
        }

        /// <summary>
        /// Obtém uma tag pelo slug
        /// </summary>
        /// <param name="slug">Slug da tag</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Dados da tag</returns>
        [HttpGet("by-slug/{slug}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Tag>> GetTagBySlug(string slug, CancellationToken cancellationToken)
        {
            try
            {
                var tag = await _tagRepository.GetBySlugAsync(slug, cancellationToken);
                if (tag == null)
                {
                    return NotFound();
                }

                return Ok(tag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter tag com slug {Slug}", slug);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar sua solicitação");
            }
        }

        /// <summary>
        /// Obtém as tags mais populares
        /// </summary>
        /// <param name="count">Número de tags a retornar (padrão: 10, máximo: 50)</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Lista de tags populares</returns>
        [HttpGet("popular")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyList<Tag>>> GetPopularTags(
            [FromQuery] int count = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var tags = await _tagRepository.GetPopularTagsAsync(
                    Math.Clamp(count, 1, 50), 
                    cancellationToken);
                return Ok(tags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter tags populares");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar sua solicitação");
            }
        }

        /// <summary>
        /// Cria uma nova tag
        /// </summary>
        /// <param name="command">Dados da tag</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Dados da tag criada</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Tag>> CreateTag([FromBody] CreateTagCommand command, CancellationToken cancellationToken)
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
                    nameof(GetTagById), 
                    new { id = result.Value.Id }, 
                    result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar tag");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar sua solicitação");
            }
        }

        /// <summary>
        /// Atualiza uma tag existente
        /// </summary>
        /// <param name="id">ID da tag</param>
        /// <param name="command">Dados atualizados da tag</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Dados da tag atualizada</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Tag>> UpdateTag(
            Guid id, 
            [FromBody] UpdateTagCommand command, 
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
                _logger.LogError(ex, "Erro ao atualizar tag com ID {TagId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar sua solicitação");
            }
        }

        /// <summary>
        /// Exclui uma tag
        /// </summary>
        /// <param name="id">ID da tag</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Resposta sem conteúdo</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteTag(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new DeleteTagCommand { Id = id };
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
                _logger.LogError(ex, "Erro ao excluir tag com ID {TagId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar sua solicitação");
            }
        }
    }
}
