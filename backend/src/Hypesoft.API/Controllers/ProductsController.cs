using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ardalis.Result;
using Hypesoft.Application.Commands.Products;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Queries.Products;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Unit = QuestPDF.Infrastructure.Unit;

namespace Hypesoft.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IMediator mediator, ILogger<ProductsController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        //Comeca aqui[HttpGet("report/pdf")]

        [HttpGet("report/pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportToPdf()
        {
            try
            {
                // Buscar produtos
                var query = new GetAllProductsQuery(null, 1, 100);
                var result = await _mediator.Send(query);

                if (result.Status != Ardalis.Result.ResultStatus.Ok)
                {
                    return StatusCode(500, "Erro ao buscar produtos");
                }

                var productsList = result.Value?.Items?.ToList() ?? new List<ProductDto>();

                // Gerar PDF usando QuestPDF
                var pdfBytes = GenerateProductReport(productsList);

                // Retornar PDF (NÃO MAIS CSV!)
                return File(
                    pdfBytes,
                    "application/pdf",
                    $"relatorio-produtos-{DateTime.Now:yyyyMMdd-HHmm}.pdf"
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, type = ex.GetType().Name });
            }
        }

        private byte[] GenerateProductReport(List<ProductDto> products)
        {
            return QuestPDF
                .Fluent.Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(QuestPDF.Helpers.PageSizes.A4);
                        page.Margin(2, QuestPDF.Infrastructure.Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        // Cabeçalho
                        page.Header()
                            .Height(100)
                            .Background(QuestPDF.Helpers.Colors.Blue.Medium)
                            .AlignCenter()
                            .Text(text =>
                            {
                                text.DefaultTextStyle(x =>
                                    x.FontColor(QuestPDF.Helpers.Colors.White)
                                );
                                text.AlignCenter();
                                text.Line("HYPESOFT CHALLENGE").FontSize(20).Bold();
                                text.Line("RELATÓRIO DE PRODUTOS").FontSize(16);
                                text.Line($"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                    .FontSize(12);
                            });

                        // Conteúdo principal
                        page.Content()
                            .PaddingVertical(1, QuestPDF.Infrastructure.Unit.Centimetre)
                            .Column(column =>
                            {
                                // Resumo executivo
                                column
                                    .Item()
                                    .ShowOnce()
                                    .Column(summary =>
                                    {
                                        summary.Item().Text("RESUMO EXECUTIVO").FontSize(16).Bold();
                                        summary
                                            .Item()
                                            .PaddingTop(10)
                                            .Row(row =>
                                            {
                                                row.RelativeItem()
                                                    .Background(
                                                        QuestPDF.Helpers.Colors.Grey.Lighten4
                                                    )
                                                    .Padding(15)
                                                    .Column(col =>
                                                    {
                                                        col.Item()
                                                            .Text(
                                                                $"Total de Produtos: {products.Count}"
                                                            )
                                                            .FontSize(12)
                                                            .Bold();
                                                        col.Item()
                                                            .Text(
                                                                $"Produtos Disponíveis: {products.Count(p => p.StockQuantity > 0)}"
                                                            )
                                                            .FontSize(11);
                                                        col.Item()
                                                            .Text(
                                                                $"Produtos Esgotados: {products.Count(p => p.StockQuantity == 0)}"
                                                            )
                                                            .FontSize(11);
                                                        col.Item()
                                                            .Text(
                                                                $"Valor Total em Estoque: R$ {products.Sum(p => p.Price * p.StockQuantity):N2}"
                                                            )
                                                            .FontSize(11);
                                                    });
                                            });
                                    });

                                column
                                    .Item()
                                    .PaddingTop(20)
                                    .Text("DETALHAMENTO DOS PRODUTOS")
                                    .FontSize(16)
                                    .Bold();

                                // Tabela de produtos
                                column
                                    .Item()
                                    .PaddingTop(10)
                                    .Table(table =>
                                    {
                                        // Definir colunas
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(3); // Nome
                                            columns.RelativeColumn(2); // Categoria
                                            columns.RelativeColumn(1); // Preço
                                            columns.RelativeColumn(1); // Estoque
                                            columns.RelativeColumn(1); // Status
                                        });

                                        // Cabeçalho da tabela
                                        table.Header(header =>
                                        {
                                            header
                                                .Cell()
                                                .Background(QuestPDF.Helpers.Colors.Grey.Medium)
                                                .Padding(8)
                                                .Text("PRODUTO")
                                                .FontColor(QuestPDF.Helpers.Colors.White)
                                                .Bold();
                                            header
                                                .Cell()
                                                .Background(QuestPDF.Helpers.Colors.Grey.Medium)
                                                .Padding(8)
                                                .Text("CATEGORIA")
                                                .FontColor(QuestPDF.Helpers.Colors.White)
                                                .Bold();
                                            header
                                                .Cell()
                                                .Background(QuestPDF.Helpers.Colors.Grey.Medium)
                                                .Padding(8)
                                                .Text("PREÇO")
                                                .FontColor(QuestPDF.Helpers.Colors.White)
                                                .Bold();
                                            header
                                                .Cell()
                                                .Background(QuestPDF.Helpers.Colors.Grey.Medium)
                                                .Padding(8)
                                                .Text("ESTOQUE")
                                                .FontColor(QuestPDF.Helpers.Colors.White)
                                                .Bold();
                                            header
                                                .Cell()
                                                .Background(QuestPDF.Helpers.Colors.Grey.Medium)
                                                .Padding(8)
                                                .Text("STATUS")
                                                .FontColor(QuestPDF.Helpers.Colors.White)
                                                .Bold();
                                        });

                                        // Linhas da tabela
                                        foreach (var product in products)
                                        {
                                            var status =
                                                product.StockQuantity > 0
                                                    ? "Disponível"
                                                    : "Esgotado";
                                            var statusColor =
                                                product.StockQuantity > 0
                                                    ? QuestPDF.Helpers.Colors.Green.Medium
                                                    : QuestPDF.Helpers.Colors.Red.Medium;

                                            table
                                                .Cell()
                                                .Border(1)
                                                .Padding(5)
                                                .Text(product.Name ?? "")
                                                .FontSize(9);
                                            table
                                                .Cell()
                                                .Border(1)
                                                .Padding(5)
                                                .Text(product.CategoryName ?? "N/A")
                                                .FontSize(9);
                                            table
                                                .Cell()
                                                .Border(1)
                                                .Padding(5)
                                                .Text($"R$ {product.Price:F2}")
                                                .FontSize(9);
                                            table
                                                .Cell()
                                                .Border(1)
                                                .Padding(5)
                                                .Text(product.StockQuantity.ToString())
                                                .FontSize(9);
                                            table
                                                .Cell()
                                                .Border(1)
                                                .Padding(5)
                                                .Text(status)
                                                .FontColor(statusColor)
                                                .FontSize(9);
                                        }
                                    });
                            });

                        // Rodapé
                        page.Footer()
                            .Height(30)
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.CurrentPageNumber();
                                x.Span(" / ");
                                x.TotalPages();
                            });
                    });
                })
                .GeneratePdf();
        }

        // Acaba aqui ai vem o Resto dos seus métodos permanecem iguais...
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

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
        {
            try
            {
                _logger.LogInformation("=== INÍCIO CREATE PRODUCT ===");

                if (command == null)
                    return BadRequest("Dados do produto não fornecidos");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _mediator.Send(command);

                if (result.Status == ResultStatus.Invalid)
                    return BadRequest(result.ValidationErrors);

                if (result.Status == ResultStatus.NotFound)
                    return NotFound("Categoria não encontrada");

                if (result.IsSuccess)
                    return CreatedAtAction(
                        nameof(GetById),
                        new { id = result.Value },
                        new { id = result.Value }
                    );

                return StatusCode(500, "Resultado inesperado ao criar produto");
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

        [HttpPut("{id:guid}")]
        //[Authorize(Roles = "products-update")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCommand command)
        {
            try
            {
                var commandWithId = command with { Id = id };

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _mediator.Send(commandWithId);

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

        [HttpDelete("{id:guid}")]
        //[Authorize(Roles = "products-delete")]
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
