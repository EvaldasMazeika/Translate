using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace translate.web.ViewModels
{
    public class NewPostViewModel
    {
        [Required]
        [MinLength(5)]
        [MaxLength(50)]
        [Display(Name ="Pavadinimas")]
        public string Title { get; set; }

        [Required]
        [MinLength(5)]
        [Display(Name = "Pranešimas")]
        public string Message { get; set; }

        [Display(Name = "Svarbus pranešimas")]
        public bool IsImportant { get; set; }
    }
}
