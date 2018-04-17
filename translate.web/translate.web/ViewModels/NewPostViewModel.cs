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
        [Display(Name = "postTitle")]
        public string Title { get; set; }

        [Required]
        [MinLength(5)]
        [MaxLength(500)]
        [Display(Name = "postMessage")]
        public string Message { get; set; }

        [Display(Name = "postIsImportantBoolean")]
        public bool IsImportant { get; set; }
    }
}
