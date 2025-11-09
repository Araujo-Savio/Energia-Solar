using System.ComponentModel.DataAnnotations;

namespace SolarEnergy.ViewModels
{
    public class QuoteChatViewModel
    {
        public int QuoteId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string CurrentUserType { get; set; } = string.Empty; // "Client" ou "Company"
        public string Status { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public int MonthlyConsumptionKwh { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public string? InitialMessage { get; set; }
        public List<ChatMessageViewModel> Messages { get; set; } = new();
        public string? CompanyResponseMessage { get; set; }
        public DateTime? CompanyResponseDate { get; set; }
        public bool HasProposal { get; set; }
        public List<ProposalViewModel> Proposals { get; set; } = new();
    }

    public class ChatMessageViewModel
    {
        public int MessageId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime SentDate { get; set; }
        public string SenderType { get; set; } = string.Empty; // "Client" ou "Company"
        public string SenderName { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public bool IsCurrentUser { get; set; }
    }

    public class SendChatMessageViewModel
    {
        [Required]
        public int QuoteId { get; set; }

        [Required(ErrorMessage = "A mensagem é obrigatória")]
        [StringLength(2000, ErrorMessage = "A mensagem deve ter no máximo 2000 caracteres")]
        [Display(Name = "Mensagem")]
        public string Message { get; set; } = string.Empty;
    }
}