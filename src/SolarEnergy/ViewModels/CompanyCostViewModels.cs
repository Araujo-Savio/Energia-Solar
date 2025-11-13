using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SolarEnergy.Models;

namespace SolarEnergy.ViewModels
{
    public class CompanyCostProfileEditViewModel
    {
        [Required]
        public string CompanyId { get; set; } = string.Empty;

        [Display(Name = "Geração média por kWp (kWh/mês)")]
        [Range(10, 400)]
        public double ProductionPerKilowattPeak { get; set; } = 140.0;

        [Display(Name = "Taxa anual de manutenção (%)")]
        [Range(0, 100)]
        public decimal MaintenanceRatePercent { get; set; } = 1.5m;

        [Display(Name = "Tarifa de aluguel (R$/kWh)")]
        [Range(0, 5)]
        public decimal RentalRatePerKwh { get; set; } = 0.65m;

        [Display(Name = "Reajuste anual do aluguel (%)")]
        [Range(0, 100)]
        public decimal RentalAnnualIncreasePercent { get; set; } = 3.0m;

        public IList<CompanyCostItemViewModel> EquipmentCosts { get; set; } = new List<CompanyCostItemViewModel>();

        public IList<CompanyCostItemViewModel> ServiceCosts { get; set; } = new List<CompanyCostItemViewModel>();

        public IList<CompanySystemSizeCostViewModel> SystemSizeCosts { get; set; } = new List<CompanySystemSizeCostViewModel>();
    }

    public class CompanyCostItemViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(120)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Categoria")]
        public CompanyCostItemType ItemType { get; set; }

        [Display(Name = "Custo (R$)")]
        [Range(typeof(decimal), "0", "999999999999")]
        public decimal Cost { get; set; }

        [MaxLength(60)]
        public string? Unit { get; set; }

        [MaxLength(300)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class CompanySystemSizeCostViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(40)]
        public string Label { get; set; } = string.Empty;

        [Display(Name = "Potência (kWp)")]
        [Range(typeof(decimal), "1", "1000")]
        public decimal SystemSizeKwp { get; set; }

        [Display(Name = "Custo médio (R$)")]
        [Range(typeof(decimal), "0", "999999999999")]
        public decimal AverageCost { get; set; }

        [MaxLength(200)]
        public string? Notes { get; set; }
    }

    public class CompanyCostSummaryViewModel
    {
        public string CompanyId { get; set; } = string.Empty;
        public double ProductionPerKilowattPeak { get; set; }
        public decimal MaintenanceRate { get; set; }
        public decimal RentalRatePerKwh { get; set; }
        public decimal RentalAnnualIncrease { get; set; }
        public IList<CompanyCostItemViewModel> EquipmentCosts { get; set; } = new List<CompanyCostItemViewModel>();
        public IList<CompanyCostItemViewModel> ServiceCosts { get; set; } = new List<CompanyCostItemViewModel>();
        public IList<CompanySystemSizeCostViewModel> SystemSizeCosts { get; set; } = new List<CompanySystemSizeCostViewModel>();
    }
}
