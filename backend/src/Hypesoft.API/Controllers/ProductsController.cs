using System;
using System.Threading.Tasks;
using Ardalis.Result;
using Hypesoft.Application.Commands;
using Hypesoft.Application.Commands.Products;
using Hypesoft.Application.Queries;
using Hypesoft.Application.Queries.Categories;
using Hypesoft.Application.Queries.Products;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hypesoft.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IMediator mediator, ILogger<ProductsController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtém uma lista paginada de produtos
        /// </summary>
        /// <param name="search">Termo de busca opcional</param>
        /// <param name="pageNumber">Número da página (padrão: 1)</param>
        /// <param name="pageSize">Itens por página (padrão: 10, máximo: 100)</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
        )
        {
            try
            {
                pageNumber = Math.Max(1, pageNumber);
                pageSize = Math.Clamp(pageSize, 1, 100);

                var query = new GetAllProductsQuery(search, pageNumber, pageSize);
                var result = await _mediator.Send(query);

                return result.Status == ResultStatus.NotFound ? NotFound() : Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar produtos");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro ao buscar os produtos"
                );
            }
        }

        /// <summary>
        /// Obtém um produto pelo ID
        /// </summary>
        /// <param name="id">ID do produto</param>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var query = new GetProductByIdQuery(id);
                var result = await _mediator.Send(query);

                return result.Status == ResultStatus.NotFound ? NotFound() : Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar produto com ID {ProductId}", id);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    $"Ocorreu um erro ao buscar o produto com ID {id}"
                );
            }
        }

        /// <summary>
        /// Cria um novo produto
        /// </summary>
        /// <param name="command">Dados do produto</param>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _mediator.Send(command);

                if (result.Status == ResultStatus.Invalid)
                    return BadRequest(result.ValidationErrors);

                if (result.Status == ResultStatus.NotFound)
                    return NotFound("Categoria não encontrada");

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.Value },
                    new { id = result.Value }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar produto");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro ao criar o produto"
                );
            }
        }

        /// <summary>
        /// Atualiza um produto existente
        /// </summary>
        /// <param name="id">ID do produto</param>
        /// <param name="command">Dados atualizados do produto</param>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCommand command)
        {
            try
            {
                if (id != command.Id)
                    return BadRequest("ID na URL não corresponde ao ID no corpo da requisição");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _mediator.Send(command);

                if (result.Status == ResultStatus.NotFound)
                    return NotFound("Produto ou categoria não encontrada");

                if (result.Status == ResultStatus.Invalid)
                    return BadRequest(result.ValidationErrors);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar produto com ID {ProductId}", id);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    $"Ocorreu um erro ao atualizar o produto com ID {id}"
                );
            }
        }

        /// <summary>
        /// Remove um produto
        /// </summary>
        /// <param name="id">ID do produto a ser removido</param>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var command = new DeleteProductCommand(id);
                var result = await _mediator.Send(command);

                if (result.Status == ResultStatus.NotFound)
                    return NotFound("Produto não encontrado");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover produto com ID {ProductId}", id);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    $"Ocorreu um erro ao remover o produto com ID {id}"
                );
            }
        }
    }
}
