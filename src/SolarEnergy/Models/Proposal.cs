using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolarEnergy.Models
{
    public class Proposal
    {
        [Key]
        public int ProposalId { get; set; }

        [Required]
        public int QuoteId { get; set; }

        [Required]
        [Display(Name = "Valor da Proposta")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Value { get; set; }

        [Display(Name = "Descrição da Proposta")]
        [StringLength(2000, ErrorMessage = "A descrição deve ter no máximo 2000 caracteres")]
        public string? Description { get; set; }

        [Display(Name = "Prazo de Instalação (dias)")]
        [Range(1, 365, ErrorMessage = "O prazo deve ser entre 1 e 365 dias")]
        public int? InstallationTimeframe { get; set; }

        [Display(Name = "Garantia (anos)")]
        [Range(1, 50, ErrorMessage = "A garantia deve ser entre 1 e 50 anos")]
        public int? Warranty { get; set; }

        [Display(Name = "Economia Estimada Mensal")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? EstimatedMonthlySavings { get; set; }

        [Required]
        [Display(Name = "Data da Proposta")]
        public DateTime ProposalDate { get; set; } = DateTime.Now;

        [Display(Name = "Válida Até")]
        public DateTime? ValidUntil { get; set; }

        [Required]
        [Display(Name = "Status")]
        [StringLength(50)]
        public string Status { get; set; } = "Enviada";

        // Navigation property
        [ForeignKey("QuoteId")]
        public virtual Quote Quote { get; set; } = null!;
    }

    public enum ProposalStatus
    {
        [Display(Name = "Enviada")]
        Sent,

        [Display(Name = "Visualizada")]
        Viewed,

        [Display(Name = "Aceita")]
        Accepted,

        [Display(Name = "Recusada")]
        Rejected,

        [Display(Name = "Expirada")]
        Expired
    }
}