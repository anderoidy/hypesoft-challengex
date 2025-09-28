using System.Text;
using Hypesoft.Application.DTOs;
using iText.Html2pdf;
using iText.Kernel.Pdf;

namespace Hypesoft.Infrastructure.Services
{
    public interface IPdfReportService
    {
        Task<byte[]> GenerateProductsReportAsync(ProductsReportDto reportData);
    }

    public class PdfReportService : IPdfReportService
    {
        public async Task<byte[]> GenerateProductsReportAsync(ProductsReportDto reportData)
        {
            var html = GenerateHtmlReport(reportData);

            using var stream = new MemoryStream();
            var converterProperties = new ConverterProperties();

            HtmlConverter.ConvertToPdf(html, stream, converterProperties);

            return stream.ToArray();
        }

        private string GenerateHtmlReport(ProductsReportDto reportData)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='UTF-8'>");
            sb.AppendLine("<title>Relatório de Produtos - Hypesoft</title>");
            sb.AppendLine("<style>");
            sb.AppendLine(
                @"
                body { font-family: Arial, sans-serif; margin: 20px; }
                .header { text-align: center; margin-bottom: 30px; }
                .company-name { color: #2563eb; font-size: 24px; font-weight: bold; }
                .report-title { font-size: 20px; margin: 10px 0; }
                .report-date { color: #666; font-size: 14px; }
                .summary { background: #f8fafc; padding: 20px; border-radius: 8px; margin: 20px 0; }
                .summary-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 20px; }
                .summary-item { text-align: center; }
                .summary-value { font-size: 24px; font-weight: bold; color: #2563eb; }
                .summary-label { color: #666; font-size: 14px; }
                table { width: 100%; border-collapse: collapse; margin: 20px 0; }
                th, td { border: 1px solid #ddd; padding: 12px; text-align: left; }
                th { background-color: #2563eb; color: white; }
                tr:nth-child(even) { background-color: #f2f2f2; }
                .status-badge { padding: 4px 12px; border-radius: 20px; font-size: 12px; font-weight: bold; }
                .status-low { background: #fef2f2; color: #dc2626; }
                .status-ok { background: #f0fdf4; color: #16a34a; }
                .price { font-weight: bold; color: #059669; }
                .footer { margin-top: 30px; text-align: center; color: #666; font-size: 12px; }
            "
            );
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            // Header
            sb.AppendLine("<div class='header'>");
            sb.AppendLine("<div class='company-name'>HYPESOFT</div>");
            sb.AppendLine("<div class='report-title'>Relatório de Produtos</div>");
            sb.AppendLine(
                $"<div class='report-date'>Gerado em: {reportData.GeneratedAt:dd/MM/yyyy HH:mm}</div>"
            );
            sb.AppendLine("</div>");

            // Summary
            sb.AppendLine("<div class='summary'>");
            sb.AppendLine("<div class='summary-grid'>");
            sb.AppendLine(
                $"<div class='summary-item'><div class='summary-value'>{reportData.TotalProducts}</div><div class='summary-label'>Total de Produtos</div></div>"
            );
            sb.AppendLine(
                $"<div class='summary-item'><div class='summary-value'>R$ {reportData.TotalValue:N2}</div><div class='summary-label'>Valor Total do Estoque</div></div>"
            );
            sb.AppendLine(
                $"<div class='summary-item'><div class='summary-value'>{reportData.LowStockProducts}</div><div class='summary-label'>Produtos com Estoque Baixo</div></div>"
            );
            sb.AppendLine(
                $"<div class='summary-item'><div class='summary-value'>{reportData.ProductsByCategory.Count}</div><div class='summary-label'>Categorias</div></div>"
            );
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

            // Products Table
            sb.AppendLine("<table>");
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th>Produto</th>");
            sb.AppendLine("<th>Categoria</th>");
            sb.AppendLine("<th>Preço</th>");
            sb.AppendLine("<th>Estoque</th>");
            sb.AppendLine("<th>Valor Total</th>");
            sb.AppendLine("<th>Status</th>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</thead>");
            sb.AppendLine("<tbody>");

            foreach (var product in reportData.Products)
            {
                var statusClass = product.Stock < 10 ? "status-low" : "status-ok";
                var totalValue = product.Price * product.Stock;

                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{product.Name}</td>");
                sb.AppendLine($"<td>{product.CategoryName}</td>");
                sb.AppendLine($"<td class='price'>R$ {product.Price:N2}</td>");
                sb.AppendLine($"<td>{product.Stock} unidades</td>");
                sb.AppendLine($"<td class='price'>R$ {totalValue:N2}</td>");
                sb.AppendLine(
                    $"<td><span class='status-badge {statusClass}'>{product.Status}</span></td>"
                );
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");

            // Footer
            sb.AppendLine("<div class='footer'>");
            sb.AppendLine(
                "© 2025 Hypesoft Challenge. Relatório gerado automaticamente pelo sistema."
            );
            sb.AppendLine("</div>");

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }
    }
}
