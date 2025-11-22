using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolarEnergy.Models
{
    public class CompanyParameters
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string CompanyId { get; set; } = string.Empty;

        // ===== CAMPOS ATUAIS (existem no banco) =====

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Preço do sistema por kWp (R$/kWp)")]
        public decimal SystemPricePerKwp { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Manutenção anual (% do investimento)")]
        public decimal MaintenancePercent { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Desconto na instalação (%)")]
        public decimal InstallDiscountPercent { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Fator da mensalidade (% da conta)")]
        public decimal RentalFactorPercent { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Mensalidade mínima (R$)")]
        public decimal RentalMinMonthly { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Taxa de setup por kWp (R$)")]
        public decimal RentalSetupPerKwp { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Reajuste anual do aluguel (%)")]
        public decimal RentalAnnualIncreasePercent { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Desconto na conta (%)")]
        public decimal RentalDiscountPercent { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Consumo mensal por 1 kWp (kWh)")]
        public decimal ConsumptionPerKwp { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Potência mínima do sistema (kWp)")]
        public decimal MinSystemSizeKwp { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ===== Relação com ApplicationUser =====
        [ForeignKey(nameof(CompanyId))]
        public ApplicationUser Company { get; set; } = null!;
    }
}
