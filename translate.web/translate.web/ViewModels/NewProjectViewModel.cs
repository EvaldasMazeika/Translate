using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace translate.web.ViewModels
{
    public class NewProjectViewModel
    {
        [Required(ErrorMessage = "required")]
        [Display(Name = "postTitle")]
        [MaxLength(20, ErrorMessage = "tooLong")]
        public string Name { get; set; }

        [Required(ErrorMessage = "required")]
        [Display(Name = "newProjectDescription")]
        [MaxLength(100, ErrorMessage = "tooLong")]
        public string Description { get; set; }


        [DataType(DataType.Upload)]
        public IFormFile File { get; set; }

        [Display(Name = "documentLanguage")]
        public Guid LanguageId { get; set; }

    }
}
