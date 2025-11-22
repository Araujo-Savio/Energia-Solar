using System.ComponentModel.DataAnnotations;

namespace SolarEnergy.ViewModels
{
    public class CompanyParametersViewModel
    {
        public int Id { get; set; }

        public string CompanyId { get; set; } = string.Empty;

        [Display(Name = "Preço do sistema por kWp (R$/kWp)")]
        public decimal SystemPricePerKwp { get; set; }

        [Display(Name = "Manutenção anual (% do investimento)")]
        public decimal MaintenancePercent { get; set; }

        [Display(Name = "Desconto na instalação (%)")]
        public decimal InstallDiscountPercent { get; set; }

        [Display(Name = "Fator da mensalidade (% da conta)")]
        public decimal RentalFactorPercent { get; set; }

        [Display(Name = "Taxa de setup por kWp (R$)")]
        public decimal RentalSetupPerKwp { get; set; }

        [Display(Name = "Mensalidade mínima (R$)")]
        public decimal RentalMinMonthly { get; set; }

        [Display(Name = "Reajuste anual do aluguel (%)")]
        public decimal RentalAnnualIncreasePercent { get; set; }

        [Display(Name = "Desconto na conta (%)")]
        public decimal RentalDiscountPercent { get; set; }

        [Display(Name = "Consumo mensal por 1 kWp (kWh)")]
        public decimal ConsumptionPerKwp { get; set; }

        [Display(Name = "Potência mínima do sistema (kWp)")]
        public decimal MinSystemSizeKwp { get; set; }
    }
}
