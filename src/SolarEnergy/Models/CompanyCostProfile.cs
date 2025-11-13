using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SolarEnergy.Models
{
    public class CompanyCostProfile
    {
        public int Id { get; set; }

        [Required]
        public string CompanyId { get; set; } = string.Empty;

        public ApplicationUser? Company { get; set; }

        [Range(10, 400)]
        public double ProductionPerKilowattPeak { get; set; } = 140.0;

        [Range(0, 1)]
        public decimal MaintenanceRate { get; set; } = 0.015m;

        [Range(0, 5)]
        public decimal RentalRatePerKwh { get; set; } = 0.65m;

        [Range(0, 1)]
        public decimal RentalAnnualIncrease { get; set; } = 0.03m;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<CompanyCostItem> CostItems { get; set; } = new List<CompanyCostItem>();

        public ICollection<CompanySystemSizeCost> SystemSizeCosts { get; set; } = new List<CompanySystemSizeCost>();
    }
}
