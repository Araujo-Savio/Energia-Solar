using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolarEnergy.Models
{
    public class TechnicalVisit
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [StringLength(450)]
        public string CompanyId { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string ClientId { get; set; } = string.Empty;

        [Required]
        [StringLength(60)]
        public string ServiceType { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "date")]
        public DateTime VisitDate { get; set; }

        [Required]
        [Column(TypeName = "time")]
        public TimeSpan VisitTime { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        public string? Notes { get; set; }

        [Required]
        public TechnicalVisitStatus Status { get; set; } = TechnicalVisitStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(CompanyId))]
        public ApplicationUser Company { get; set; } = null!;

        [ForeignKey(nameof(ClientId))]
        public ApplicationUser Client { get; set; } = null!;
    }

    public enum TechnicalVisitStatus
    {
        Pending = 1,
        Confirmed = 2,
        Done = 3
    }
}
