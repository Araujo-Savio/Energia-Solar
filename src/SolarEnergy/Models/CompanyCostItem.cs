using System.ComponentModel.DataAnnotations;

namespace SolarEnergy.Models
{
    public class CompanyCostItem
    {
        public int Id { get; set; }

        [Required]
        public int CompanyCostProfileId { get; set; }

        public CompanyCostProfile? Profile { get; set; }

        [Required]
        [MaxLength(120)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(60)]
        public string? Unit { get; set; }

        [Required]
        public CompanyCostItemType ItemType { get; set; }

        [Range(typeof(decimal), "0", "999999999999")]
        public decimal Cost { get; set; }

        [MaxLength(300)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public enum CompanyCostItemType
    {
        Equipment = 0,
        Service = 1,
        Other = 2
    }
}
