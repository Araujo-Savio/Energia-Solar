using System.Globalization;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using SolarEnergy.ViewModels;

namespace SolarEnergy.Services
{
    public class SimulationExportService : ISimulationExportService
    {
        public byte[] ExportarSimulacaoParaPdf(SimulationResultsDto resultados)
        {
            using var memoryStream = new MemoryStream();
            using var pdfWriter = new PdfWriter(memoryStream);
            using var pdfDocument = new PdfDocument(pdfWriter);
            using var document = new Document(pdfDocument, PageSize.A4);

            document.SetMargins(36, 36, 36, 36);

            var culture = new CultureInfo("pt-BR");

            document.Add(new Paragraph("Relatório da Simulação de Energia Solar")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(18)
                .SetBold());

            document.Add(new Paragraph($"Gerado em {DateTime.UtcNow:dd/MM/yyyy HH:mm} (UTC)")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(10)
                .SetFontColor(ColorConstants.GRAY));

            AddKeyValueSection(document, "Informações gerais", new (string, string?)[]
            {
                ("Perfil", resultados.IsCompanyUser ? "Empresa" : "Usuário"),
                ("Empresa selecionada", string.IsNullOrWhiteSpace(resultados.SelectedCompanyName) ? "-" : resultados.SelectedCompanyName)
            });

            AddKeyValueSection(document, "Dados informados na calculadora", new (string, string?)[]
            {
                ("Consumo médio mensal (kWh)", FormatNumber(resultados.CalculatorAverageMonthlyConsumptionKwh, culture, " kWh")),
                ("Tarifa de energia (R$/kWh)", FormatCurrency(resultados.CalculatorTariffPerKwh, culture)),
                ("Cobertura desejada (%)", FormatPercent(resultados.CalculatorCoveragePercent, culture)),
                ("Degradação anual (%)", FormatPercent(resultados.CalculatorDegradationPercent, culture)),
                ("Horizonte de análise (anos)", resultados.CalculatorHorizonYears.ToString(culture)),
                ("Inflação da tarifa informada (%)", FormatPercent(resultados.CalculatorTariffInflationPercent, culture))
            });

            if (resultados.CompanyParameters is not null)
            {
                var p = resultados.CompanyParameters;
                AddKeyValueSection(document, "Parâmetros da empresa", new (string, string?)[]
                {
                    ("Preço do sistema por kWp (R$)", FormatNumber(p.SystemPricePerKwp, culture)),
                    ("Manutenção anual (% do investimento)", FormatPercent(p.MaintenancePercent, culture)),
                    ("Desconto na instalação (%)", FormatPercent(p.InstallDiscountPercent, culture)),
                    ("Fator da mensalidade do aluguel (%)", FormatPercent(p.RentalFactorPercent, culture)),
                    ("Taxa de instalação do aluguel (R$ por kWp/projeto)", FormatNumber(p.RentalSetupPerKwp, culture)),
                    ("Mensalidade mínima de aluguel (R$)", FormatNumber(p.RentalMinMonthly, culture)),
                    ("Reajuste anual da mensalidade (%)", FormatPercent(p.RentalAnnualIncreasePercent, culture)),
                    ("Desconto aplicado na conta (%)", FormatPercent(p.RentalDiscountPercent, culture)),
                    ("Consumo mensal por kWp (kWh)", FormatNumber(p.ConsumptionPerKwp, culture, " kWh")),
                    ("Potência mínima do sistema (kWp)", FormatNumber(p.MinSystemSizeKwp, culture, " kWp"))
                });
            }

            var resultadoSimulacao = resultados.IsCompanyUser
                ? (UserSimulationResult?)resultados.CompanyResult
                : resultados.UserResult;

            if (resultadoSimulacao is not null)
            {
                AddKeyValueSection(document, "Resumo dos resultados", new (string, string?)[]
                {
                    ("Custo sem energia solar", FormatCurrency(resultadoSimulacao.CostWithoutSolar, culture)),
                    ("Custo com aluguel", FormatCurrency(resultadoSimulacao.RentCost, culture)),
                    ("Investimento com instalação", FormatCurrency(resultadoSimulacao.InstallationInvestment, culture)),
                    ("Economia com instalação", FormatCurrency(resultadoSimulacao.InstallationSavings, culture)),
                    ("Economia com aluguel", FormatCurrency(resultadoSimulacao.RentSavings, culture)),
                    ("Energia gerada anualmente (kWh)", FormatNumber(resultadoSimulacao.AnnualGeneratedEnergyKwh, culture, " kWh")),
                    ("Cobertura estimada da conta (%)", FormatPercent(resultadoSimulacao.CoveragePercent, culture)),
                    ("Horizonte analisado (anos)", resultadoSimulacao.AnalyzedHorizonYears.ToString(culture))
                });

                AddKeyValueSection(document, "Cenário de instalação própria", new (string, string?)[]
                {
                    ("Investimento inicial", FormatCurrency(resultadoSimulacao.InitialInvestment, culture)),
                    ("Economia mensal estimada", FormatCurrency(resultadoSimulacao.MonthlySavingInstallation, culture)),
                    ("Economia total no horizonte", FormatCurrency(resultadoSimulacao.InstallationTotalSavingHorizon, culture)),
                    ("Tempo de instalação (meses)", FormatNumber(resultadoSimulacao.InstallationTimeMonths, culture)),
                    ("Payback estimado (anos)", resultadoSimulacao.PaybackYears?.ToString("N2", culture)),
                    ("Economia acumulada em 5 anos", FormatCurrency(resultadoSimulacao.FiveYearAccumulatedSaving, culture)),
                    ("Investimento recuperado em (anos)", resultadoSimulacao.InvestmentRecoveryYears.ToString("N2", culture))
                });

                AddKeyValueSection(document, "Cenário de aluguel de energia", new (string, string?)[]
                {
                    ("Mensalidade inicial", FormatCurrency(resultadoSimulacao.InitialRentAmount, culture)),
                    ("Economia mensal estimada", FormatCurrency(resultadoSimulacao.MonthlySavingRent, culture)),
                    ("Economia total no horizonte", FormatCurrency(resultadoSimulacao.RentTotalSavingHorizon, culture)),
                    ("Desconto aplicado na conta (%)", FormatPercent(resultadoSimulacao.DiscountAppliedPercent, culture)),
                    ("Economia média anual", FormatCurrency(resultadoSimulacao.AverageAnnualSaving, culture)),
                    ("Total projetado com instalação", FormatCurrency(resultadoSimulacao.TotalInstallCost, culture)),
                    ("Total projetado com aluguel", FormatCurrency(resultadoSimulacao.TotalRentalCost, culture))
                });
            }
            else
            {
                document.Add(new Paragraph("Nenhum resultado de simulação foi fornecido para exportação.")
                    .SetFontColor(ColorConstants.RED)
                    .SetMarginTop(12));
            }

            document.Close();
            return memoryStream.ToArray();
        }

        private static void AddKeyValueSection(Document document, string title, IEnumerable<(string Label, string? Value)> rows)
        {
            document.Add(new Paragraph(title)
                .SetFontSize(13)
                .SetBold()
                .SetMarginTop(12)
                .SetMarginBottom(6));

            var table = new Table(UnitValue.CreatePercentArray(new float[] { 1, 2 }))
                .UseAllAvailableWidth();

            foreach (var (label, value) in rows)
            {
                table.AddCell(new Cell().Add(new Paragraph(label).SetBold()).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
                table.AddCell(new Cell().Add(new Paragraph(string.IsNullOrWhiteSpace(value) ? "-" : value)));
            }

            document.Add(table);
        }

        private static string FormatCurrency(double value, CultureInfo culture) => value.ToString("C", culture);
        private static string FormatCurrency(decimal value, CultureInfo culture) => value.ToString("C", culture);
        private static string FormatNumber(double value, CultureInfo culture, string suffix = "") => $"{value.ToString("N2", culture)}{suffix}";
        private static string FormatNumber(decimal value, CultureInfo culture, string suffix = "") => $"{value.ToString("N2", culture)}{suffix}";
        private static string FormatPercent(double value, CultureInfo culture) => $"{value.ToString("N2", culture)}%";
        private static string FormatPercent(decimal value, CultureInfo culture) => $"{value.ToString("N2", culture)}%";
    }
}
