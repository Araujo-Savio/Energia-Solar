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
        public decimal SystemPricePerKwp { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal MaintenancePercent { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal InstallDiscountPercent { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal RentalFactorPercent { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RentalMinMonthly { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RentalSetupPerKwp { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal RentalAnnualIncreasePercent { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal RentalDiscountPercent { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ConsumptionPerKwp { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MinSystemSizeKwp { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ===== Relação com ApplicationUser =====
        [ForeignKey(nameof(CompanyId))]
        public ApplicationUser Company { get; set; } = null!;
    }
}
