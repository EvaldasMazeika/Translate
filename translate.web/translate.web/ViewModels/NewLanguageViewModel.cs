using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace translate.web.ViewModels
{
    public class NewLanguageViewModel
    {
        [Required]
        [MinLength(5)]
        [MaxLength(100)]
        [Display(Name = "Pavadinimas")]
        public string Name { get; set; }

    }
}
