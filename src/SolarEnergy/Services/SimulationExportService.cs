using System.Globalization;
using System.IO;
using System.Text;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using SolarEnergy.ViewModels;

namespace SolarEnergy.Services
{
    public class SimulationExportService : ISimulationExportService
    {
        public string GenerateUserCsv(UserSimulationInput input, UserSimulationResult result)
        {
            var ptBr = new CultureInfo("pt-BR");
            var sb = new StringBuilder();

            sb.AppendLine("Métrica;Valor");

            sb.AppendLine($"Custo sem energia solar;{result.CostWithoutSolar.ToString("C", ptBr)}");
            sb.AppendLine($"Investimento com instalação;{result.InstallationInvestment.ToString("C", ptBr)}");
            sb.AppendLine($"Custo com aluguel;{result.RentCost.ToString("C", ptBr)}");
            sb.AppendLine($"Economia com instalação;{result.InstallationSavings.ToString("C", ptBr)}");
            sb.AppendLine($"Economia com aluguel;{result.RentSavings.ToString("C", ptBr)}");
            sb.AppendLine($"Energia gerada anualmente (kWh);{result.AnnualGeneratedEnergyKwh.ToString("N2", ptBr)}");
            sb.AppendLine($"Economia mensal (instalação);{result.MonthlySavingInstallation.ToString("C", ptBr)}");
            sb.AppendLine($"Economia mensal (aluguel);{result.MonthlySavingRent.ToString("C", ptBr)}");
            sb.AppendLine($"Economia média anual;{result.AverageAnnualSaving.ToString("C", ptBr)}");
            sb.AppendLine($"Payback estimado (anos);{result.PaybackYears.ToString("N2", ptBr)}");
            sb.AppendLine($"Tempo de instalação (meses);{result.InstallationTimeMonths.ToString("N1", ptBr)}");
            sb.AppendLine($"Mensalidade de aluguel inicial;{result.InitialRentAmount.ToString("C", ptBr)}");
            sb.AppendLine($"Desconto aplicado na conta;{result.DiscountAppliedPercent.ToString("N2", ptBr)}%");
            sb.AppendLine($"Economia acumulada em 5 anos;{result.FiveYearAccumulatedSaving.ToString("C", ptBr)}");
            sb.AppendLine($"Horizonte analisado (anos);{result.AnalyzedHorizonYears}");
            sb.AppendLine($"Cobertura considerada (%);{result.CoveragePercent.ToString("N2", ptBr)}");

            return sb.ToString();
        }

        public byte[] GenerateUserPdf(UserSimulationInput input, UserSimulationResult result)
        {
            var culture = new CultureInfo("pt-BR");
            using var memoryStream = new MemoryStream();
            using var writer = new PdfWriter(memoryStream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            var title = new Paragraph("Simulação de Economia - Resultado")
                .SetFont(boldFont)
                .SetFontSize(18)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10);
            document.Add(title);

            if (!string.IsNullOrWhiteSpace(result.SelectedCompanyName))
            {
                var companyInfo = new Paragraph($"Empresa selecionada: {result.SelectedCompanyName}")
                    .SetFont(normalFont)
                    .SetFontSize(11)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(5);
                document.Add(companyInfo);
            }

            var inputInfo = new Paragraph(
                    $"Consumo médio: {input.AverageMonthlyConsumptionKwh.ToString("N0", culture)} kWh | Tarifa: {input.TariffPerKwh.ToString("C", culture)} | Cobertura: {input.CoveragePercent.ToString("N2", culture)}% | Horizonte: {input.HorizonYears} anos")
                .SetFont(normalFont)
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(15);
            document.Add(inputInfo);

            var table = new Table(2).UseAllAvailableWidth();
            table.AddHeaderCell(new Cell().Add(new Paragraph("Métrica").SetFont(boldFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Valor").SetFont(boldFont)));

            AddRow(table, "Custo sem energia solar", result.CostWithoutSolar.ToString("C", culture), normalFont);
            AddRow(table, "Investimento com instalação", result.InstallationInvestment.ToString("C", culture), normalFont);
            AddRow(table, "Custo com aluguel", result.RentCost.ToString("C", culture), normalFont);
            AddRow(table, "Economia com instalação", result.InstallationSavings.ToString("C", culture), normalFont);
            AddRow(table, "Economia com aluguel", result.RentSavings.ToString("C", culture), normalFont);
            AddRow(table, "Energia gerada anualmente (kWh)", result.AnnualGeneratedEnergyKwh.ToString("N2", culture), normalFont);
            AddRow(table, "Economia mensal (instalação)", result.MonthlySavingInstallation.ToString("C", culture), normalFont);
            AddRow(table, "Economia mensal (aluguel)", result.MonthlySavingRent.ToString("C", culture), normalFont);
            AddRow(table, "Economia média anual", result.AverageAnnualSaving.ToString("C", culture), normalFont);
            AddRow(table, "Payback estimado", result.PaybackYears.HasValue ? $"{result.PaybackYears.Value.ToString("N2", culture)} anos" : "Não recupera investimento", normalFont);
            AddRow(table, "Tempo de instalação", $"{result.InstallationTimeMonths.ToString("N1", culture)} meses", normalFont);
            AddRow(table, "Mensalidade de aluguel inicial", result.InitialRentAmount.ToString("C", culture), normalFont);
            AddRow(table, "Desconto aplicado na conta", $"{result.DiscountAppliedPercent.ToString("N2", culture)}%", normalFont);
            AddRow(table, "Economia acumulada em 5 anos", result.FiveYearAccumulatedSaving.ToString("C", culture), normalFont);
            AddRow(table, "Horizonte analisado", $"{result.AnalyzedHorizonYears} anos", normalFont);
            AddRow(table, "Cobertura considerada", $"{result.CoveragePercent.ToString("N2", culture)}%", normalFont);

            document.Add(table);

            document.Close();
            return memoryStream.ToArray();
        }

        private static void AddRow(Table table, string label, string value, PdfFont font)
        {
            table.AddCell(new Cell().Add(new Paragraph(label).SetFont(font)));
            table.AddCell(new Cell().Add(new Paragraph(value).SetFont(font)));
        }
    }
}
