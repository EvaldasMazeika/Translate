using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace translate.web.ViewModels
{
    public class ResetPasswordViewModel
    {
        public string Code { get; set; }


        [Display(Name = "registerEmail")]
        public string Email { get; set; }

        [Required(ErrorMessage = "required")]
        [MaxLength(50, ErrorMessage = "tooLong")]
        [MinLength(8, ErrorMessage = "tooShort")]
        [DataType(DataType.Password)]
        [Display(Name = "registerPassword")]
        public string Password { get; set; }

        [Required(ErrorMessage = "required")]
        [MaxLength(50, ErrorMessage = "tooLong")]
        [MinLength(8, ErrorMessage = "tooShort")]
        [DataType(DataType.Password)]
        [Display(Name = "repeatPassword")]
        public string ConfirmPassword { get; set; }
    }
}
