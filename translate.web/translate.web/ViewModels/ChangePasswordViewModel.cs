using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace translate.web.ViewModels
{
    public class ChangePasswordViewModel
    {
        [DataType(DataType.Password)]
        [Display(Name = "currentPassword")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "required")]
        [MinLength(8, ErrorMessage = "tooShort")]
        [MaxLength(50, ErrorMessage = "tooLong")]
        [DataType(DataType.Password)]
        [Display(Name = "newPassword")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "required")]
        [MinLength(8, ErrorMessage = "tooShort")]
        [MaxLength(50, ErrorMessage = "tooLong")]
        [DataType(DataType.Password)]
        [Display(Name = "repeatPassword")]
        public string RepeatPassword { get; set; }

        public bool HasPassword { get; set; }
    }
}
