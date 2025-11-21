using SolarEnergy.ViewModels;
using System.Text;
using System.Globalization;

namespace SolarEnergy.Services
{
    public class SimpleExportService : IExportService
    {
        public Task<byte[]> ExportToPdfAsync(MonthlyReportViewModel report)
        {
            try
            {
                // Generate a simple HTML report that can be saved as HTML file
                var html = GenerateHtmlReport(report);
                
                // Convert to UTF-8 bytes with proper BOM for better browser compatibility
                var preamble = Encoding.UTF8.GetPreamble();
                var content = Encoding.UTF8.GetBytes(html);
                var result = new byte[preamble.Length + content.Length];
                Array.Copy(preamble, 0, result, 0, preamble.Length);
                Array.Copy(content, 0, result, preamble.Length, content.Length);
                
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                // Return basic text report as last resort
                var fallbackText = $"RELATÓRIO MENSAL\nEmpresa: {report.CompanyName}\nPeríodo: {report.ReportMonth:MMMM/yyyy}\n\nLeads: {report.LeadMetrics.TotalLeadsReceived}\nReceita: {report.FinancialMetrics.TotalRevenue:C}\n\nErro na geração: {ex.Message}";
                return Task.FromResult(Encoding.UTF8.GetBytes(fallbackText));
            }
        }

        public Task<byte[]> ExportToCsvAsync(MonthlyReportViewModel report)
        {
            try
            {
                var csv = new StringBuilder();
                var culture = new CultureInfo("pt-BR");
                
                // Header
                csv.AppendLine($"RELATÓRIO MENSAL - {report.CompanyName}");
                csv.AppendLine($"Período: {report.ReportMonth.ToString("MMMM/yyyy", culture)}");
                csv.AppendLine();

                // KPIs
                csv.AppendLine("PRINCIPAIS INDICADORES");
                csv.AppendLine("Métrica;Valor Atual;Mês Anterior;Variação %");
                csv.AppendLine($"Leads Recebidos;{report.LeadMetrics.TotalLeadsReceived};{report.LeadMetrics.PreviousMonthLeads};{report.LeadMetrics.LeadsGrowthPercentage:F1}");
                csv.AppendLine($"Receita Total;{report.FinancialMetrics.TotalRevenue.ToString("C", culture)};{report.FinancialMetrics.PreviousMonthRevenue.ToString("C", culture)};{report.FinancialMetrics.RevenueGrowthPercentage:F1}");
                csv.AppendLine($"Avaliação Média;{report.SatisfactionMetrics.AverageRating:F1};{report.SatisfactionMetrics.PreviousMonthRating:F1};{report.SatisfactionMetrics.RatingImprovement:F1}");
                csv.AppendLine();

                // Lead Analysis
                csv.AppendLine("ANÁLISE DE LEADS");
                csv.AppendLine("Métrica;Valor");
                csv.AppendLine($"Leads Orgânicos;{report.LeadMetrics.OrganicLeads}");
                csv.AppendLine($"Leads Pagos;{report.LeadMetrics.PaidLeads}");
                csv.AppendLine($"Investimento em Leads;{report.LeadMetrics.LeadPurchaseInvestment.ToString("C", culture)}");
                csv.AppendLine($"Taxa de Conversão;{report.LeadMetrics.ConversionRate:F1}%");
                csv.AppendLine();

                // Performance
                csv.AppendLine("PERFORMANCE OPERACIONAL");
                csv.AppendLine("Métrica;Valor");
                csv.AppendLine($"Tempo Médio de Resposta;{report.PerformanceMetrics.AverageResponseTime.Hours}h {report.PerformanceMetrics.AverageResponseTime.Minutes}m");
                csv.AppendLine($"Taxa de Resposta;{report.PerformanceMetrics.ResponseRate:F1}%");
                csv.AppendLine($"Propostas Enviadas;{report.PerformanceMetrics.TotalProposalsSent}");
                csv.AppendLine($"Taxa de Aceitação;{report.PerformanceMetrics.ProposalAcceptanceRate:F1}%");
                csv.AppendLine();

                // Reviews
                if (report.SatisfactionMetrics.RecentReviews?.Any() == true)
                {
                    csv.AppendLine("AVALIAÇÕES RECENTES");
                    csv.AppendLine("Cliente;Rating;Data;Comentário");
                    foreach (var review in report.SatisfactionMetrics.RecentReviews.Take(5))
                    {
                        var cleanComment = SanitizeText(review.Comment).Replace(";", ",");
                        csv.AppendLine($"{review.ClientName};{review.Rating};{review.Date:dd/MM/yyyy};{cleanComment}");
                    }
                }

                csv.AppendLine();
                csv.AppendLine($"Relatório gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}");

                // Convert with BOM for Excel compatibility
                var preamble = Encoding.UTF8.GetPreamble();
                var content = Encoding.UTF8.GetBytes(csv.ToString());
                var result = new byte[preamble.Length + content.Length];
                Array.Copy(preamble, 0, result, 0, preamble.Length);
                Array.Copy(content, 0, result, preamble.Length, content.Length);

                return Task.FromResult(result);
            }
            catch (Exception)
            {
                // Return basic CSV as fallback
                var fallbackCsv = $"Empresa;{report.CompanyName}\nPeriodo;{report.ReportMonth:yyyy-MM}\nLeads;{report.LeadMetrics.TotalLeadsReceived}\nReceita;{report.FinancialMetrics.TotalRevenue}";
                return Task.FromResult(Encoding.UTF8.GetBytes(fallbackCsv));
            }
        }

        private string GenerateHtmlReport(MonthlyReportViewModel report)
        {
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='pt-BR'>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='utf-8'>");
            html.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1'>");
            html.AppendLine("<title>Relatório Mensal</title>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: Arial, sans-serif; margin: 40px; line-height: 1.6; }");
            html.AppendLine("h1, h2 { color: #FF6B35; text-align: center; }");
            html.AppendLine("table { width: 100%; border-collapse: collapse; margin: 20px 0; }");
            html.AppendLine("th, td { padding: 12px; text-align: left; border: 1px solid #ddd; }");
            html.AppendLine("th { background-color: #f2f2f2; font-weight: bold; }");
            html.AppendLine(".section { margin: 30px 0; }");
            html.AppendLine(".footer { text-align: center; margin-top: 40px; font-size: 12px; color: #666; }");
            html.AppendLine(".header { text-align: center; margin-bottom: 30px; }");
            html.AppendLine("</style>");
            html.AppendLine("</head><body>");

            // Header
            html.AppendLine("<div class='header'>");
            html.AppendLine($"<h1>RELATÓRIO MENSAL</h1>");
            html.AppendLine($"<h2>{SanitizeText(report.CompanyName)}</h2>");
            html.AppendLine($"<p><strong>Período:</strong> {report.ReportMonth:MMMM/yyyy}</p>");
            html.AppendLine("</div>");
            html.AppendLine("<hr>");

            // KPI Section
            html.AppendLine("<div class='section'>");
            html.AppendLine("<h2>INDICADORES PRINCIPAIS</h2>");
            html.AppendLine("<table>");
            html.AppendLine("<tr><th>MÉTRICA</th><th>VALOR ATUAL</th><th>MÊS ANTERIOR</th><th>VARIAÇÃO</th></tr>");
            
            html.AppendLine($"<tr>");
            html.AppendLine($"<td>Leads Recebidos</td>");
            html.AppendLine($"<td>{report.LeadMetrics.TotalLeadsReceived}</td>");
            html.AppendLine($"<td>{report.LeadMetrics.PreviousMonthLeads}</td>");
            html.AppendLine($"<td>{report.LeadMetrics.LeadsGrowthPercentage:+0.0;-0.0;0.0}%</td>");
            html.AppendLine($"</tr>");

            html.AppendLine($"<tr>");
            html.AppendLine($"<td>Receita Total</td>");
            html.AppendLine($"<td>{report.FinancialMetrics.TotalRevenue:C}</td>");
            html.AppendLine($"<td>{report.FinancialMetrics.PreviousMonthRevenue:C}</td>");
            html.AppendLine($"<td>{report.FinancialMetrics.RevenueGrowthPercentage:+0.0;-0.0;0.0}%</td>");
            html.AppendLine($"</tr>");

            html.AppendLine($"<tr>");
            html.AppendLine($"<td>Avaliação Média</td>");
            html.AppendLine($"<td>{report.SatisfactionMetrics.AverageRating:F1} estrelas</td>");
            html.AppendLine($"<td>{report.SatisfactionMetrics.PreviousMonthRating:F1} estrelas</td>");
            html.AppendLine($"<td>{report.SatisfactionMetrics.RatingImprovement:+0.0;-0.0;0.0}</td>");
            html.AppendLine($"</tr>");
            
            html.AppendLine("</table>");
            html.AppendLine("</div>");

            // Lead Analysis
            html.AppendLine("<div class='section'>");
            html.AppendLine("<h2>ANÁLISE DE LEADS</h2>");
            html.AppendLine("<table>");
            html.AppendLine("<tr><th>MÉTRICA</th><th>VALOR</th></tr>");
            html.AppendLine($"<tr><td>Leads Orgânicos</td><td>{report.LeadMetrics.OrganicLeads}</td></tr>");
            html.AppendLine($"<tr><td>Leads Pagos</td><td>{report.LeadMetrics.PaidLeads}</td></tr>");
            html.AppendLine($"<tr><td>Investimento em Leads</td><td>{report.LeadMetrics.LeadPurchaseInvestment:C}</td></tr>");
            html.AppendLine($"<tr><td>Taxa de Conversão</td><td>{report.LeadMetrics.ConversionRate:F1}%</td></tr>");
            html.AppendLine("</table>");
            html.AppendLine("</div>");

            // Performance Section
            html.AppendLine("<div class='section'>");
            html.AppendLine("<h2>PERFORMANCE OPERACIONAL</h2>");
            html.AppendLine("<table>");
            html.AppendLine("<tr><th>MÉTRICA</th><th>VALOR</th></tr>");
            html.AppendLine($"<tr><td>Tempo Médio de Resposta</td><td>{report.PerformanceMetrics.AverageResponseTime.Hours}h {report.PerformanceMetrics.AverageResponseTime.Minutes}m</td></tr>");
            html.AppendLine($"<tr><td>Taxa de Resposta</td><td>{report.PerformanceMetrics.ResponseRate:F1}%</td></tr>");
            html.AppendLine($"<tr><td>Propostas Enviadas</td><td>{report.PerformanceMetrics.TotalProposalsSent}</td></tr>");
            html.AppendLine($"<tr><td>Taxa de Aceitação</td><td>{report.PerformanceMetrics.ProposalAcceptanceRate:F1}%</td></tr>");
            html.AppendLine("</table>");
            html.AppendLine("</div>");

            // Reviews Section
            if (report.SatisfactionMetrics.RecentReviews?.Any() == true)
            {
                html.AppendLine("<div class='section'>");
                html.AppendLine("<h2>AVALIAÇÕES RECENTES</h2>");
                
                foreach (var review in report.SatisfactionMetrics.RecentReviews.Take(5))
                {
                    var stars = new string('?', Math.Max(0, Math.Min(5, review.Rating)));
                    html.AppendLine("<div style='margin: 10px 0; padding: 10px; border-left: 3px solid #FF6B35;'>");
                    html.AppendLine($"<strong>{SanitizeText(review.ClientName)}</strong> - {stars} ({review.Rating}/5)");
                    html.AppendLine($"<br><small>Data: {review.Date:dd/MM/yyyy}</small>");
                    html.AppendLine($"<br>\"{SanitizeText(review.Comment)}\"");
                    html.AppendLine("</div>");
                }
                html.AppendLine("</div>");
            }

            // Footer
            html.AppendLine("<hr>");
            html.AppendLine($"<div class='footer'>Relatório gerado em {DateTime.Now:dd/MM/yyyy HH:mm}</div>");
            html.AppendLine("</body></html>");

            return html.ToString();
        }

        private static string SanitizeText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "Sem comentário";

            return text.Replace("\r\n", " ")
                      .Replace("\n", " ")
                      .Replace("\r", " ")
                      .Replace("\"", "&quot;")
                      .Replace("<", "&lt;")
                      .Replace(">", "&gt;")
                      .Replace("&", "&amp;")
                      .Substring(0, Math.Min(text.Length, 200));
        }
    }
}