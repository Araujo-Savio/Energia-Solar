using System.ComponentModel.DataAnnotations;
using SolarEnergy.Models;

namespace SolarEnergy.ViewModels
{
    public class LeadBalanceViewModel
    {
        public int AvailableLeads { get; set; }
        public int ConsumedLeads { get; set; }
        public int TotalPurchasedLeads { get; set; }
        public DateTime? LastPurchaseDate { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class LeadPurchaseViewModel
    {
        [Required]
        [Display(Name = "Pacote de Leads")]
        public LeadPackageType PackageType { get; set; }

        [Display(Name = "Quantidade")]
        public int Quantity => (int)PackageType;

        [Display(Name = "Preço Unitário")]
        public decimal UnitPrice { get; set; } = 14.99m;

        [Display(Name = "Desconto")]
        public decimal DiscountPercentage { get; set; }

        [Display(Name = "Valor Total")]
        public decimal TotalPrice { get; set; }

        [Display(Name = "Economia")]
        public decimal Savings { get; set; }
    }

    public class LeadPackageOption
    {
        public LeadPackageType Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal Savings { get; set; }
        public bool IsPopular { get; set; }
        public string Badge { get; set; } = string.Empty;
    }

    public class PurchaseLeadsViewModel
    {
        public LeadBalanceViewModel Balance { get; set; } = new();
        public List<LeadPackageOption> PackageOptions { get; set; } = new();
        public List<LeadPurchaseHistoryViewModel> RecentPurchases { get; set; } = new();
    }

    public class LeadPurchaseHistoryViewModel
    {
        public int Id { get; set; }
        public int LeadQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
    }

    public class LeadConsumptionViewModel
    {
        public int Id { get; set; }
        public int QuoteId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public int MonthlyConsumptionKwh { get; set; }
        public DateTime ConsumedAt { get; set; }
        public DateTime QuoteRequestDate { get; set; }
    }

    public class LeadManagementViewModel
    {
        public LeadBalanceViewModel Balance { get; set; } = new();
        public List<LeadPurchaseHistoryViewModel> PurchaseHistory { get; set; } = new();
        public List<LeadConsumptionViewModel> ConsumptionHistory { get; set; } = new();
        public decimal TotalInvestment { get; set; }
        public decimal AverageLeadCost { get; set; }
        public int LeadsThisMonth { get; set; }
        public int ConsumedThisMonth { get; set; }
    }

    public class MaskedQuoteDetailsViewModel
    {
        public int QuoteId { get; set; }
        public string MaskedClientName { get; set; } = string.Empty;
        public string MaskedClientEmail { get; set; } = string.Empty;
        public string MaskedClientPhone { get; set; } = string.Empty;
        public string MaskedClientLocation { get; set; } = string.Empty;
        public int MonthlyConsumptionKwh { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public string? Message { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool HasAccess { get; set; }
        public bool CanPurchaseAccess { get; set; }
        public int AvailableLeads { get; set; }
    }
}