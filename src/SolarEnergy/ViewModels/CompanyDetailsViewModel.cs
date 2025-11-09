using System.ComponentModel.DataAnnotations;

namespace SolarEnergy.ViewModels
{
    public class CompanyDetailsViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? LegalName { get; set; }
        public string? TradeName { get; set; }
        public string? Location { get; set; }
        public string? Phone { get; set; }
        public string? Website { get; set; }
        public string? Description { get; set; }
        public string? ProfileImagePath { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public List<CompanyReviewViewModel> Reviews { get; set; } = new();
        public bool CanUserReview { get; set; }
        public bool HasUserAlreadyRated { get; set; }
        public bool HasExistingQuote { get; set; }
        public int? ExistingQuoteId { get; set; }
    }

    public class CompanyReviewViewModel
    {
        public int Id { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public int? Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsCurrentUserReview { get; set; }
    }

    public class AddReviewViewModel
    {
        [Required(ErrorMessage = "O comentário é obrigatório")]
        [StringLength(1000, ErrorMessage = "O comentário deve ter no máximo 1000 caracteres")]
        [Display(Name = "Comentário")]
        public string Comment { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "A avaliação deve ser entre 1 e 5 estrelas")]
        [Display(Name = "Avaliação")]
        public int? Rating { get; set; }

        public string CompanyId { get; set; } = string.Empty;
    }
}