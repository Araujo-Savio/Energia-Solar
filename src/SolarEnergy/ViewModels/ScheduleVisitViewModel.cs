using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SolarEnergy.ViewModels
{
    public class ScheduleVisitViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Selecione a empresa responsável pela visita.")]
        [Display(Name = "Empresa")]
        public string? CompanyId { get; set; }

        [Required(ErrorMessage = "Informe o tipo de serviço.")]
        [StringLength(60, ErrorMessage = "O tipo de serviço pode ter no máximo 60 caracteres.")]
        [Display(Name = "Tipo de Serviço")]
        public string ServiceType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a data da visita.")]
        [DataType(DataType.Date)]
        [Display(Name = "Data da Visita")]
        public DateTime? VisitDate { get; set; }

        [Required(ErrorMessage = "Informe o horário da visita.")]
        [DataType(DataType.Time)]
        [Display(Name = "Horário")]
        public TimeSpan? VisitTime { get; set; }

        [StringLength(200, ErrorMessage = "O endereço pode ter no máximo 200 caracteres.")]
        [Display(Name = "Endereço")]
        public string? Address { get; set; }

        [Display(Name = "Observações")]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        public IEnumerable<SelectListItem> Companies { get; set; } = Enumerable.Empty<SelectListItem>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (VisitDate.HasValue && VisitDate.Value.Date < DateTime.Today)
            {
                yield return new ValidationResult(
                    "A data da visita não pode ser no passado.",
                    new[] { nameof(VisitDate) });
            }

            if (VisitTime.HasValue)
            {
                var min = new TimeSpan(8, 0, 0);
                var max = new TimeSpan(18, 0, 0);
                if (VisitTime.Value < min || VisitTime.Value > max)
                {
                    yield return new ValidationResult(
                        "O horário deve estar entre 08:00 e 18:00.",
                        new[] { nameof(VisitTime) });
                }
            }
        }
    }
}
