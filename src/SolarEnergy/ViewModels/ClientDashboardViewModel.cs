using System.ComponentModel.DataAnnotations;

namespace SolarEnergy.ViewModels
{
    public class ClientDashboardViewModel
    {
        public string ClientName { get; set; } = string.Empty;
        public ClientStatisticsViewModel Statistics { get; set; } = new();
        public List<QuoteListViewModel> RecentQuotes { get; set; } = new();
        public List<ActivityItemViewModel> RecentActivities { get; set; } = new();
    }

    public class ClientStatisticsViewModel
    {
        public int TotalQuotes { get; set; }
        public int PendingQuotes { get; set; }
        public int ReceivedQuotes { get; set; }
        public int AcceptedQuotes { get; set; }
    }

    public class ActivityItemViewModel
    {
        public ActivityType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? RelatedCompanyName { get; set; }
        public int? RelatedQuoteId { get; set; }
        public string IconClass => Type switch
        {
            ActivityType.QuoteRequested => "fas fa-file-plus",
            ActivityType.QuoteReceived => "fas fa-envelope",
            ActivityType.ProposalReceived => "fas fa-handshake",
            ActivityType.ReviewSubmitted => "fas fa-star",
            ActivityType.MessageSent => "fas fa-comment",
            ActivityType.MessageReceived => "fas fa-comment-dots",
            _ => "fas fa-info-circle"
        };

        public string IconColor => Type switch
        {
            ActivityType.QuoteRequested => "bg-solar-orange",
            ActivityType.QuoteReceived => "bg-success",
            ActivityType.ProposalReceived => "bg-info",
            ActivityType.ReviewSubmitted => "bg-warning",
            ActivityType.MessageSent => "bg-primary",
            ActivityType.MessageReceived => "bg-secondary",
            _ => "bg-muted"
        };

        public string GetTimeAgo()
        {
            var timeSpan = DateTime.Now - Date;

            if (timeSpan.TotalMinutes < 1)
                return "Agora";
            if (timeSpan.TotalMinutes < 60)
                return $"Há {(int)timeSpan.TotalMinutes} min";
            if (timeSpan.TotalHours < 24)
                return $"Há {(int)timeSpan.TotalHours}h";
            if (timeSpan.TotalDays < 7)
                return $"Há {(int)timeSpan.TotalDays} dias";
            
            return Date.ToString("dd/MM/yyyy");
        }
    }

    public enum ActivityType
    {
        [Display(Name = "Orçamento solicitado")]
        QuoteRequested,
        
        [Display(Name = "Orçamento recebido")]
        QuoteReceived,
        
        [Display(Name = "Proposta recebida")]
        ProposalReceived,
        
        [Display(Name = "Avaliação enviada")]
        ReviewSubmitted,
        
        [Display(Name = "Mensagem enviada")]
        MessageSent,
        
        [Display(Name = "Mensagem recebida")]
        MessageReceived
    }
}