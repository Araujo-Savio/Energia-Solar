using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolarEnergy.Models
{
    public class CompanyLeadBalance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string CompanyId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Leads Disponíveis")]
        public int AvailableLeads { get; set; } = 0;

        [Required]
        [Display(Name = "Leads Consumidos")]
        public int ConsumedLeads { get; set; } = 0;

        [Required]
        [Display(Name = "Total de Leads Comprados")]
        public int TotalPurchasedLeads { get; set; } = 0;

        [Required]
        [Display(Name = "Data de Criação")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Última Atualização")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("CompanyId")]
        public virtual ApplicationUser Company { get; set; } = null!;

        public virtual ICollection<LeadPurchase> LeadPurchases { get; set; } = new List<LeadPurchase>();
        public virtual ICollection<LeadConsumption> LeadConsumptions { get; set; } = new List<LeadConsumption>();
    }

    public class LeadPurchase
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string CompanyId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Quantidade de Leads")]
        public int LeadQuantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Preço Unitário")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Desconto Percentual")]
        public decimal DiscountPercentage { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Valor Total")]
        public decimal TotalAmount { get; set; }

        [Required]
        [Display(Name = "Data da Compra")]
        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        [Display(Name = "Status do Pagamento")]
        public string PaymentStatus { get; set; } = "Completed"; // Para desenvolvimento

        [StringLength(100)]
        [Display(Name = "ID da Transação")]
        public string? TransactionId { get; set; }

        [StringLength(500)]
        [Display(Name = "Observações")]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("CompanyId")]
        public virtual ApplicationUser Company { get; set; } = null!;
    }

    public class LeadConsumption
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string CompanyId { get; set; } = string.Empty;

        [Required]
        public int QuoteId { get; set; }

        [Required]
        [Display(Name = "Data do Consumo")]
        public DateTime ConsumedAt { get; set; } = DateTime.Now;

        [StringLength(200)]
        [Display(Name = "Observações")]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("CompanyId")]
        public virtual ApplicationUser Company { get; set; } = null!;

        [ForeignKey("QuoteId")]
        public virtual Quote Quote { get; set; } = null!;
    }

    public enum LeadPackageType
    {
        [Display(Name = "1 Lead")]
        Single = 1,

        [Display(Name = "Pacote 20 Leads (5% desconto)")]
        Pack20 = 20,

        [Display(Name = "Pacote 50 Leads (10% desconto)")]
        Pack50 = 50,

        [Display(Name = "Pacote 100 Leads (15% desconto)")]
        Pack100 = 100
    }
}