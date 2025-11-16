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
        public decimal PricePerKwP { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AnnualMaintenance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal InstallationDiscount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RentalPercent { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MinRentalValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RentalSetupFee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AnnualRentIncrease { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RentDiscount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal KwhPerKwp { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MinSystemPower { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
