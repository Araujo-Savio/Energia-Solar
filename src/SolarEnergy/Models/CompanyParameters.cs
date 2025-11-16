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

        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerKwp { get; set; } = 4200m;

        [Column(TypeName = "decimal(5,2)")]
        public decimal MaintenancePercent { get; set; } = 1.2m;

        [Column(TypeName = "decimal(5,2)")]
        public decimal InstallDiscountPercent { get; set; } = 4m;

        [Column(TypeName = "decimal(5,2)")]
        public decimal RentalFactorPercent { get; set; } = 68m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal RentalMinMonthly { get; set; } = 250m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal RentalSetupPerKwp { get; set; } = 150m;

        [Column(TypeName = "decimal(5,2)")]
        public decimal RentalAnnualIncreasePercent { get; set; } = 4.5m;

        [Column(TypeName = "decimal(5,2)")]
        public decimal RentalDiscountPercent { get; set; } = 15m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal ConsumptionPerKwp { get; set; } = 120m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal MinSystemSizeKwp { get; set; } = 2.5m;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(CompanyId))]
        public ApplicationUser Company { get; set; } = null!;
    }
}
