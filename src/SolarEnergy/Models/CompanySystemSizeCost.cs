using System.ComponentModel.DataAnnotations;

namespace SolarEnergy.Models
{
    public class CompanySystemSizeCost
    {
        public int Id { get; set; }

        [Required]
        public int CompanyCostProfileId { get; set; }

        public CompanyCostProfile? Profile { get; set; }

        [Required]
        [MaxLength(40)]
        public string Label { get; set; } = string.Empty;

        [Range(typeof(decimal), "1", "1000")]
        public decimal SystemSizeKwp { get; set; }

        [Range(typeof(decimal), "0", "999999999999")] // limites realistas para validação
        public decimal AverageCost { get; set; }

        [MaxLength(200)]
        public string? Notes { get; set; }
    }
}
