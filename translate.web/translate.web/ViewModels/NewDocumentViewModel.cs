using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace translate.web.ViewModels
{
    public class NewDocumentViewModel
    {

        [Required]
        [MinLength(2)]
        [MaxLength(50)]
        [Display(Name="documentType")]
        public string Name { get; set; }

        [Required]
        [Display(Name ="documentExample")]
        public string Example { get; set; }
    }
}
