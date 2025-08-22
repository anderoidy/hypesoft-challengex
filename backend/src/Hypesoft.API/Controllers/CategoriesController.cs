using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Hypesoft.Application.Commands.Categories;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Queries.Categories;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
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
            ILogger<CategoriesController> logger
        )
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _categoryRepository =
                categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtém todas as categorias com suporte a paginação
        /// </summary>
        /// <param name="pageNumber">Número da página (padrão: 1)</param>
        /// <param name="pageSize">Itens por página (padrão: 20, máximo: 100)</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20
        )
        {
            try
            {
                pageNumber = Math.Max(1, pageNumber);
                pageSize = Math.Clamp(pageSize, 1, 100);

                var query = new GetAllCategoriesQuery(null, pageNumber, pageSize);

                var result = await _mediator.Send(query);

                return result.Status == ResultStatus.NotFound ? NotFound() : Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar categorias");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro ao buscar as categorias"
                );
            }
        }

        /// <summary>
        /// Obtém uma categoria pelo ID
        /// </summary>
        /// <param name="id">ID da categoria</param>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var query = new GetCategoryByIdQuery(id);
                var result = await _mediator.Send(query);

                return result.Status == ResultStatus.NotFound ? NotFound() : Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar categoria com ID {CategoryId}", id);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro ao buscar a categoria"
                );
            }
        }

        /// <summary>
        /// Cria uma nova categoria
        /// </summary>
        /// <param name="command">Dados da categoria</param>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateCategoryCommand command)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _mediator.Send(command);

                if (result.Status == ResultStatus.Invalid)
                    return BadRequest(result.ValidationErrors);

                if (result.Status == ResultStatus.NotFound)
                    return NotFound("Categoria pai não encontrada");

                return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar categoria");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro ao criar a categoria"
                );
            }
        }

        /// <summary>
        /// Atualiza uma categoria existente
        /// </summary>
        /// <param name="id">ID da categoria</param>
        /// <param name="command">Dados atualizados da categoria</param>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryCommand command)
        {
            try
            {
                if (id != command.Id)
                    return BadRequest("ID na URL não corresponde ao ID no corpo da requisição");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _mediator.Send(command);

                if (result.Status == ResultStatus.NotFound)
                    return NotFound("Categoria não encontrada");

                if (result.Status == ResultStatus.Invalid)
                    return BadRequest(result.ValidationErrors);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar categoria com ID {CategoryId}", id);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro ao atualizar a categoria"
                );
            }
        }

        /// <summary>
        /// Remove uma categoria
        /// </summary>
        /// <param name="id">ID da categoria a ser removida</param>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var command = new DeleteCategoryCommand(id);

                var result = await _mediator.Send(command);

                if (result.Status == ResultStatus.NotFound)
                    return NotFound("Categoria não encontrada");

                if (result.Status == ResultStatus.Invalid)
                    return BadRequest(result.ValidationErrors);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover categoria com ID {CategoryId}", id);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro ao remover a categoria"
                );
            }
        }
    }
}
