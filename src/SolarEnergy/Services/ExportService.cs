using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Layout.Borders;
using SolarEnergy.ViewModels;
using System.Text;
using System.Globalization;

namespace SolarEnergy.Services
{
    public interface IExportService
    {
        Task<byte[]> ExportToPdfAsync(MonthlyReportViewModel report);
        Task<byte[]> ExportToCsvAsync(MonthlyReportViewModel report);
    }

    public class ExportService : IExportService
    {
        public Task<byte[]> ExportToPdfAsync(MonthlyReportViewModel report)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                using var writer = new PdfWriter(memoryStream);
                using var pdf = new PdfDocument(writer);
                using var document = new Document(pdf);

                // Configure fonts
                var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                // Header
                AddHeader(document, report, boldFont, normalFont);

                // KPI Section
                AddKpiSection(document, report, boldFont, normalFont);

                // Lead Analysis
                AddLeadAnalysis(document, report, boldFont, normalFont);

                // Performance Section
                AddPerformanceSection(document, report, boldFont, normalFont);

                // Reviews Section
                if (report.SatisfactionMetrics.RecentReviews.Any())
                {
                    AddReviewsSection(document, report, boldFont, normalFont);
                }

                // Footer
                AddFooter(document, normalFont);

                document.Close();
                return Task.FromResult(memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                // Log the error and try fallback
                System.Diagnostics.Debug.WriteLine($"PDF generation error: {ex}");
                
                // Return a simple text-based PDF if there's an error
                return CreateSimplePdf(report, ex);
            }
        }

        private static void AddHeader(Document document, MonthlyReportViewModel report, PdfFont boldFont, PdfFont normalFont)
        {
            // Title
            var title = new Paragraph($"RELATÓRIO MENSAL")
                .SetFont(boldFont)
                .SetFontSize(20)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(5);
            document.Add(title);

            // Company and period
            var subtitle = new Paragraph($"{report.CompanyName}")
                .SetFont(boldFont)
                .SetFontSize(16)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(5);
            document.Add(subtitle);

            var period = new Paragraph($"Período: {report.ReportMonth:MMMM/yyyy}")
                .SetFont(normalFont)
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20);
            document.Add(period);

            // Separator line
            var line = new Paragraph("____________________________________________________")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20);
            document.Add(line);
        }

        private static void AddKpiSection(Document document, MonthlyReportViewModel report, PdfFont boldFont, PdfFont normalFont)
        {
            var sectionTitle = new Paragraph("INDICADORES PRINCIPAIS")
                .SetFont(boldFont)
                .SetFontSize(14)
                .SetMarginBottom(10);
            document.Add(sectionTitle);

            // Create table
            var table = new Table(4);
            table.SetWidth(UnitValue.CreatePercentValue(100));

            // Headers
            table.AddHeaderCell(CreateCell("MÉTRICA", boldFont, true));
            table.AddHeaderCell(CreateCell("VALOR ATUAL", boldFont, true));
            table.AddHeaderCell(CreateCell("MÊS ANTERIOR", boldFont, true));
            table.AddHeaderCell(CreateCell("VARIAÇÃO", boldFont, true));

            // Data rows
            table.AddCell(CreateCell("Leads Recebidos", normalFont));
            table.AddCell(CreateCell(report.LeadMetrics.TotalLeadsReceived.ToString(), normalFont));
            table.AddCell(CreateCell(report.LeadMetrics.PreviousMonthLeads.ToString(), normalFont));
            table.AddCell(CreateCell($"{report.LeadMetrics.LeadsGrowthPercentage:+0.0;-0.0;0.0}%", normalFont));

            table.AddCell(CreateCell("Receita Total", normalFont));
            table.AddCell(CreateCell(report.FinancialMetrics.TotalRevenue.ToString("C", new CultureInfo("pt-BR")), normalFont));
            table.AddCell(CreateCell(report.FinancialMetrics.PreviousMonthRevenue.ToString("C", new CultureInfo("pt-BR")), normalFont));
            table.AddCell(CreateCell($"{report.FinancialMetrics.RevenueGrowthPercentage:+0.0;-0.0;0.0}%", normalFont));

            table.AddCell(CreateCell("Avaliação Média", normalFont));
            table.AddCell(CreateCell($"{report.SatisfactionMetrics.AverageRating:F1} estrelas", normalFont));
            table.AddCell(CreateCell($"{report.SatisfactionMetrics.PreviousMonthRating:F1} estrelas", normalFont));
            table.AddCell(CreateCell($"{report.SatisfactionMetrics.RatingImprovement:+0.0;-0.0;0.0}", normalFont));

            document.Add(table.SetMarginBottom(20));
        }

        private static void AddLeadAnalysis(Document document, MonthlyReportViewModel report, PdfFont boldFont, PdfFont normalFont)
        {
            var sectionTitle = new Paragraph("ANÁLISE DE LEADS")
                .SetFont(boldFont)
                .SetFontSize(14)
                .SetMarginBottom(10);
            document.Add(sectionTitle);

            var table = new Table(2);
            table.SetWidth(UnitValue.CreatePercentValue(100));

            table.AddCell(CreateCell("Leads Orgânicos", normalFont));
            table.AddCell(CreateCell(report.LeadMetrics.OrganicLeads.ToString(), normalFont));

            table.AddCell(CreateCell("Leads Pagos", normalFont));
            table.AddCell(CreateCell(report.LeadMetrics.PaidLeads.ToString(), normalFont));

            table.AddCell(CreateCell("Investimento em Leads", normalFont));
            table.AddCell(CreateCell(report.LeadMetrics.LeadPurchaseInvestment.ToString("C", new CultureInfo("pt-BR")), normalFont));

            table.AddCell(CreateCell("Taxa de Conversão", normalFont));
            table.AddCell(CreateCell($"{report.LeadMetrics.ConversionRate:F1}%", normalFont));

            document.Add(table.SetMarginBottom(20));
        }

        private static void AddPerformanceSection(Document document, MonthlyReportViewModel report, PdfFont boldFont, PdfFont normalFont)
        {
            var sectionTitle = new Paragraph("PERFORMANCE OPERACIONAL")
                .SetFont(boldFont)
                .SetFontSize(14)
                .SetMarginBottom(10);
            document.Add(sectionTitle);

            var table = new Table(2);
            table.SetWidth(UnitValue.CreatePercentValue(100));

            table.AddCell(CreateCell("Tempo Médio de Resposta", normalFont));
            table.AddCell(CreateCell($"{report.PerformanceMetrics.AverageResponseTime.Hours}h {report.PerformanceMetrics.AverageResponseTime.Minutes}m", normalFont));

            table.AddCell(CreateCell("Taxa de Resposta", normalFont));
            table.AddCell(CreateCell($"{report.PerformanceMetrics.ResponseRate:F1}%", normalFont));

            table.AddCell(CreateCell("Propostas Enviadas", normalFont));
            table.AddCell(CreateCell(report.PerformanceMetrics.TotalProposalsSent.ToString(), normalFont));

            table.AddCell(CreateCell("Taxa de Aceitação", normalFont));
            table.AddCell(CreateCell($"{report.PerformanceMetrics.ProposalAcceptanceRate:F1}%", normalFont));

            document.Add(table.SetMarginBottom(20));
        }

        private static void AddReviewsSection(Document document, MonthlyReportViewModel report, PdfFont boldFont, PdfFont normalFont)
        {
            var sectionTitle = new Paragraph("AVALIAÇÕES RECENTES")
                .SetFont(boldFont)
                .SetFontSize(14)
                .SetMarginBottom(10);
            document.Add(sectionTitle);

            foreach (var review in report.SatisfactionMetrics.RecentReviews.Take(5))
            {
                var stars = new string('*', Math.Max(0, Math.Min(5, review.Rating)));
                
                var reviewBlock = new Paragraph()
                    .Add(new Text($"• {review.ClientName} - {stars} ({review.Rating}/5)").SetFont(boldFont))
                    .Add(new Text($"\n  Data: {review.Date:dd/MM/yyyy}").SetFont(normalFont).SetFontSize(9))
                    .Add(new Text($"\n  \"{SanitizeText(review.Comment)}\"").SetFont(normalFont).SetFontSize(9))
                    .SetMarginBottom(10);

                document.Add(reviewBlock);
            }
        }

        private static void AddFooter(Document document, PdfFont normalFont)
        {
            var separator = new Paragraph("____________________________________________________")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(20)
                .SetMarginBottom(10);
            document.Add(separator);

            var footer = new Paragraph($"Relatório gerado em {DateTime.Now:dd/MM/yyyy HH:mm}")
                .SetFont(normalFont)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER);
            document.Add(footer);
        }

        private static Cell CreateCell(string content, PdfFont font, bool isHeader = false)
        {
            var cell = new Cell().Add(new Paragraph(content).SetFont(font));
            
            if (isHeader)
            {
                cell.SetBackgroundColor(ColorConstants.LIGHT_GRAY);
                cell.SetFontSize(10);
            }
            else
            {
                cell.SetFontSize(9);
            }

            cell.SetPadding(5);
            cell.SetBorder(new SolidBorder(ColorConstants.GRAY, 0.5f));
            
            return cell;
        }

        private static string SanitizeText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "Sem comentário";

            // Remove problematic characters and limit length
            return text.Replace("\r\n", " ")
                      .Replace("\n", " ")
                      .Replace("\r", " ")
                      .Replace("\"", "'")
                      .Substring(0, Math.Min(text.Length, 200));
        }

        private Task<byte[]> CreateSimplePdf(MonthlyReportViewModel report, Exception ex)
        {
            // Create a very simple text-based fallback
            var sb = new StringBuilder();
            sb.AppendLine("RELATÓRIO MENSAL");
            sb.AppendLine($"Empresa: {report.CompanyName}");
            sb.AppendLine($"Período: {report.ReportMonth:MMMM/yyyy}");
            sb.AppendLine();
            sb.AppendLine("RESUMO EXECUTIVO:");
            sb.AppendLine($"- Leads Recebidos: {report.LeadMetrics.TotalLeadsReceived}");
            sb.AppendLine($"- Receita Total: {report.FinancialMetrics.TotalRevenue:C}");
            sb.AppendLine($"- Avaliação Média: {report.SatisfactionMetrics.AverageRating:F1} estrelas");
            sb.AppendLine($"- Taxa de Resposta: {report.PerformanceMetrics.ResponseRate:F1}%");
            sb.AppendLine();
            sb.AppendLine($"Observação: Erro na geração avançada do PDF - {ex.Message}");
            sb.AppendLine($"Relatório gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}");

            // Return as basic text bytes (the system will handle as PDF download)
            return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
        }

        public Task<byte[]> ExportToCsvAsync(MonthlyReportViewModel report)
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
            if (report.SatisfactionMetrics.RecentReviews.Any())
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

            return Task.FromResult(Encoding.UTF8.GetBytes(csv.ToString()));
        }
    }
}