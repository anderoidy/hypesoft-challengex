using System;
using System.Threading.Tasks;
using Ardalis.Result;
using Hypesoft.Application.Commands;
using Hypesoft.Application.Queries;
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
        /// Gets a paginated list of products with optional search term
        /// </summary>
        /// <param name="search">Optional search term to filter products</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Number of items per page (default: 10, max: 100)</param>
        /// <returns>A paginated list of products</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var query = new GetAllProductsQuery(search, pageNumber, pageSize);
                var result = await _mediator.Send(query);

                if (result.Status == ResultStatus.NotFound)
                    return NotFound(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving products");
            }
        }

        /// <summary>
        /// Gets a single product by ID
        /// </summary>
        /// <param name="id">The product ID</param>
        /// <returns>The requested product</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var query = new GetProductByIdQuery(id);
                var result = await _mediator.Send(query);

                if (result.Status == ResultStatus.NotFound)
                    return NotFound(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product with ID {ProductId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving product with ID {id}");
            }
        }

        /// <summary>
        /// Creates a new product
        /// </summary>
        /// <param name="command">The product data</param>
        /// <returns>The created product ID</returns>
        [HttpPost]
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
                    return NotFound(result);

                return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the product");
            }
        }

        /// <summary>
        /// Updates an existing product
        /// </summary>
        /// <param name="id">The product ID</param>
        /// <param name="command">The updated product data</param>
        /// <returns>No content if successful</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCommand command)
        {
            try
            {
                if (id != command.Id)
                    return BadRequest("ID in the URL does not match the ID in the request body");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _mediator.Send(command);

                if (result.Status == ResultStatus.NotFound)
                    return NotFound();

                if (result.Status == ResultStatus.Invalid)
                    return BadRequest(result.ValidationErrors);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product with ID {ProductId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while updating product with ID {id}");
            }
        }

        /// <summary>
        /// Deletes a product
        /// </summary>
        /// <param name="id">The product ID to delete</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id}")]
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
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID {ProductId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while deleting product with ID {id}");
            }
        }
    }
}