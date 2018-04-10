using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace translate.web.ViewModels
{
    public class UploadFileViewModel
    {
        [Required]
        [Display(Name ="Dokumentas")]
        [DataType(DataType.Upload)]
        public IFormFile File { get; set; }

        [Required]
        [Display(Name ="Dokumento kalba")]
        public Guid LanguageId { get; set; }

        [Required]
        [Display(Name = "Dokumento tipas")]
        public Guid DocumentTypeId { get; set; }

    }
}
