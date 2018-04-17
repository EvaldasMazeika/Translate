using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace translate.web.ViewModels
{
    public class EditMySelfViewModel
    {
        [Required(ErrorMessage = "required")]
        [MaxLength(20, ErrorMessage = "tooLong")]
        [Display(Name = "registerName")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "required")]
        [MaxLength(20, ErrorMessage = "tooLong")]
        [Display(Name = "registerSurname")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "required")]
        [MaxLength(50, ErrorMessage = "tooLong")]
        [Display(Name = "registerEmail")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.PhoneNumber)]
        [RegularExpression("^([+])([0-9]{11})$", ErrorMessage = "wrongFormat")]
        [Display(Name = "registerPhoneNumber")]
        [Required(ErrorMessage = "required")]
        public string MobileNumber { get; set; }
    }
}
