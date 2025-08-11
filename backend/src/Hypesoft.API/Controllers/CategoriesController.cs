using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Hypesoft.Application.Features.Categories.Commands;
using Hypesoft.Application.Features.Categories.Queries;
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
    public class CategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(
            IMediator mediator,
            ICategoryRepository categoryRepository,
            ILogger<CategoriesController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtém todas as categorias com suporte a paginação
        /// </summary>
        /// <param name="pageNumber">Número da página (padrão: 1)</param>
        /// <param name="pageSize">Itens por página (padrão: 20, máximo: 100)</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Lista paginada de categorias</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedList<Category>>> GetCategories(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            try
            {
                pageNumber = Math.Max(1, pageNumber);
                pageSize = Math.Clamp(pageSize, 1, 100);

                var result = await _categoryRepository.GetPaginatedCategoriesAsync(
                    pageNumber, 
                    pageSize, 
                    cancellationToken);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter categorias");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar sua solicitação");
            }
        }

        /// <summary>
        /// Obtém uma categoria pelo ID
        /// </summary>
        /// <param name="id">ID da categoria</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Dados da categoria</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Category>> GetCategoryById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var category = await _categoryRepository.GetByIdWithDetailsAsync(id, cancellationToken);
                if (category == null)
                {
                    return NotFound();
                }

                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter categoria com ID {CategoryId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar sua solicitação");
            }
        }

        /// <summary>
        /// Obtém categorias pelo slug
        /// </summary>
        /// <param name="slug">Slug da categoria</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Dados da categoria</returns>
        [HttpGet("by-slug/{slug}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Category>> GetCategoryBySlug(string slug, CancellationToken cancellationToken)
        {
            try
            {
                var category = await _categoryRepository.GetBySlugAsync(slug, cancellationToken);
                if (category == null)
                {
                    return NotFound();
                }

                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter categoria com slug {Slug}", slug);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar sua solicitação");
            }
        }

        /// <summary>
        /// Obtém a árvore de categorias
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Árvore hierárquica de categorias</returns>
        [HttpGet("tree")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IReadOnlyList<Category>>> GetCategoryTree(CancellationToken cancellationToken)
        {
            try
            {
                var categories = await _categoryRepository.GetCategoryTreeAsync(cancellationToken);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter árvore de categorias");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar sua solicitação");
            }
        }

        /// <summary>
        /// Cria uma nova categoria
        /// </summary>
        /// <param name="command">Dados da categoria</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Dados da categoria criada</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Category>> CreateCategory([FromBody] CreateCategoryCommand command, CancellationToken cancellationToken)
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
                    nameof(GetCategoryById), 
                    new { id = result.Value.Id }, 
                    result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar categoria");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar sua solicitação");
            }
        }

        /// <summary>
        /// Atualiza uma categoria existente
        /// </summary>
        /// <param name="id">ID da categoria</param>
        /// <param name="command">Dados atualizados da categoria</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Dados da categoria atualizada</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Category>> UpdateCategory(
            Guid id, 
            [FromBody] UpdateCategoryCommand command, 
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
                _logger.LogError(ex, "Erro ao atualizar categoria com ID {CategoryId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar sua solicitação");
            }
        }

        /// <summary>
        /// Exclui uma categoria
        /// </summary>
        /// <param name="id">ID da categoria</param>
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
        public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new DeleteCategoryCommand { Id = id };
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
                _logger.LogError(ex, "Erro ao excluir categoria com ID {CategoryId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar sua solicitação");
            }
        }

        /// <summary>
        /// Move uma categoria para um novo pai
        /// </summary>
        /// <param name="id">ID da categoria a ser movida</param>
        /// <param name="parentId">ID do novo pai (opcional, null para tornar raiz)</param>
        /// <param name="cancellationToken">Token de cancelamento</param>
        /// <returns>Resposta sem conteúdo em caso de sucesso</returns>
        [HttpPost("{id:guid}/move")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MoveCategory(
            Guid id,
            [FromQuery] Guid? parentId,
            CancellationToken cancellationToken)
        {
            try
            {
                var command = new MoveCategoryCommand
                {
                    CategoryId = id,
                    NewParentId = parentId
                };

                var result = await _mediator.Send(command, cancellationToken);

                if (result.Status == ResultStatus.NotFound)
                {
                    return NotFound("Categoria não encontrada");
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
                _logger.LogError(ex, "Erro ao mover categoria {CategoryId} para o pai {ParentId}", id, parentId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar sua solicitação");
            }
        }
    }
}
