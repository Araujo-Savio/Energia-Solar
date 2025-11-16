using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolarEnergy.Models
{
    public class QuoteMessage
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        public int QuoteId { get; set; }

        [Required]
        [StringLength(450)]
        public string SenderId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Mensagem")]
        [StringLength(2000, ErrorMessage = "A mensagem deve ter no máximo 2000 caracteres")]
        public string Message { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Data/Hora")]
        public DateTime SentDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Tipo de Remetente")]
        public MessageSenderType SenderType { get; set; }

        [Display(Name = "Lida em")]
        public DateTime? ReadDate { get; set; }

        // Navigation properties
        [ForeignKey("QuoteId")]
        public virtual Quote Quote { get; set; } = null!;

        [ForeignKey("SenderId")]
        public virtual ApplicationUser Sender { get; set; } = null!;

        // Propriedade computada (não mapeada para o banco)
        [NotMapped]
        public bool IsRead => ReadDate.HasValue;

        // Métodos
        public void MarkAsRead()
        {
            if (ReadDate == null)
            {
                ReadDate = DateTime.Now;
            }
        }
    }

    public enum MessageSenderType
    {
        [Display(Name = "Cliente")]
        Client,

        [Display(Name = "Empresa")]
        Company
    }
}