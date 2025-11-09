using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolarEnergy.Models
{
    public class Quote
    {
        [Key]
        public int QuoteId { get; set; }

        [Required]
        [StringLength(450)]
        public string ClientId { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string CompanyId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Consumo Mensal (kWh)")]
        [Range(1, 99999, ErrorMessage = "O consumo mensal deve ser entre 1 e 99.999 kWh")]
        public int MonthlyConsumptionKwh { get; set; }

        [Required]
        [Display(Name = "Tipo de Serviço")]
        [StringLength(50)]
        public string ServiceType { get; set; } = string.Empty;

        [Display(Name = "Mensagem")]
        [StringLength(1000, ErrorMessage = "A mensagem deve ter no máximo 1000 caracteres")]
        public string? Message { get; set; }

        [Required]
        [Display(Name = "Data da Solicitação")]
        public DateTime RequestDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Status")]
        [StringLength(50)]
        public string Status { get; set; } = "Pendente";

        // Campos para resposta da empresa
        [Display(Name = "Mensagem de Resposta da Empresa")]
        [StringLength(2000, ErrorMessage = "A mensagem de resposta deve ter no máximo 2000 caracteres")]
        public string? CompanyResponseMessage { get; set; }

        [Display(Name = "Data da Resposta da Empresa")]
        public DateTime? CompanyResponseDate { get; set; }

        // Navigation properties
        [ForeignKey("ClientId")]
        public virtual ApplicationUser Client { get; set; } = null!;

        [ForeignKey("CompanyId")]
        public virtual ApplicationUser Company { get; set; } = null!;

        // Lista de propostas relacionadas a este orçamento
        public virtual ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();

        // Lista de mensagens do chat
        public virtual ICollection<QuoteMessage> Messages { get; set; } = new List<QuoteMessage>();

        // Métodos
        public void UpdateStatus(string newStatus)
        {
            Status = newStatus;
        }

        public void SetCompanyResponse(string message)
        {
            CompanyResponseMessage = message;
            CompanyResponseDate = DateTime.Now;
            Status = "Respondido";
        }
    }

    public enum QuoteStatus
    {
        [Display(Name = "Pendente")]
        Pending,

        [Display(Name = "Em Análise")]
        InAnalysis,

        [Display(Name = "Proposta Enviada")]
        ProposalSent,

        [Display(Name = "Respondido")]
        Responded,

        [Display(Name = "Aceito")]
        Accepted,

        [Display(Name = "Recusado")]
        Rejected,

        [Display(Name = "Cancelado")]
        Canceled
    }
}