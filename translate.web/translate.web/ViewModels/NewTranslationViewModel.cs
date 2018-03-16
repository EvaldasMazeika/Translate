using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace translate.web.ViewModels
{
    public class NewTranslationViewModel
    {
        [Required]
        [Display(Name ="Dokumentas")]
        public Guid DocumentId { get; set; }

        [Required]
        [Display(Name = "Kalba")]
        public Guid LanguageId { get; set; }

        [Required]
        [Display(Name = "Pavadinimas")]
        [MinLength(5)]
        [MaxLength(20)]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Aprašymas")]
        [MinLength(5)]
        [MaxLength(100)]
        public string Description { get; set; }
    }
}
