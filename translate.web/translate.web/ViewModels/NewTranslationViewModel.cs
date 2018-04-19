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
        [Display(Name = "documentTitle")]
        public Guid DocumentId { get; set; }

        [Required]
        [Display(Name = "language")]
        public Guid LanguageId { get; set; }

        [Required]
        [Display(Name = "postTitle")]
        [MinLength(5)]
        [MaxLength(20)]
        public string Title { get; set; }

        [Required]
        [Display(Name = "projectDescription")]
        [MinLength(5)]
        [MaxLength(100)]
        public string Description { get; set; }

        [Required]
        [Display(Name = "translator")]
        public Guid TranslatorId { get; set; }
    }
}
