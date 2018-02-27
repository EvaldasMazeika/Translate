using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace translate.web.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [MaxLength(20)]
        [Display(Name = "registerName")]
        public string Name { get; set; }

        [Required]
        [MaxLength(20)]
        [Display(Name = "registerUserName")]
        public string UserName { get; set; }

        [Required]
        [MaxLength(20)]
        [Display(Name = "registerSurname")]
        public string Surname { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "registerEmail")]
        public string Email { get; set; }

        [Required]
        [MaxLength(50)]
        [MinLength(8)]
        [DataType(DataType.Password)]
        [Display(Name = "registerPassword")]
        public string Password { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "registerBirthDate")]
        public DateTime BirthDate { get; set; }

        [DataType(DataType.PhoneNumber)]
        [RegularExpression("^([+])([0-9]{11})$", ErrorMessage = "Netinkamas formatas")]
        [Display(Name = "registerPhoneNumber")]
        public string PhoneNumber { get; set; }


    }
}
