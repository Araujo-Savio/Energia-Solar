using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolarEnergy.Models
{
    public class CompanyReview
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string CompanyId { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string ReviewerId { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "A avaliação deve ser entre 1 e 5 estrelas")]
        public int? Rating { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "O comentário deve ter no máximo 1000 caracteres")]
        [Display(Name = "Comentário")]
        public string Comment { get; set; } = string.Empty;

        [Display(Name = "Data da Avaliação")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Última Atualização")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("CompanyId")]
        public virtual ApplicationUser Company { get; set; } = null!;

        [ForeignKey("ReviewerId")]
        public virtual ApplicationUser Reviewer { get; set; } = null!;
    }
}