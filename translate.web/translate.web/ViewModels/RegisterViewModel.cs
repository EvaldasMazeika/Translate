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
        [Required(ErrorMessage = "required")]
        [MaxLength(20, ErrorMessage = "tooLong")]
        [Display(Name = "registerName")]
        public string Name { get; set; }

        [Required(ErrorMessage = "required")]
        [MaxLength(20, ErrorMessage = "tooLong")]
        [Display(Name = "registerUserName")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "required")]
        [MaxLength(20, ErrorMessage = "tooLong")]
        [Display(Name = "registerSurname")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "required")]
        [MaxLength(50, ErrorMessage = "tooLong")]
        [Display(Name = "registerEmail")]
        public string Email { get; set; }

        [Required(ErrorMessage = "required")]
        [MaxLength(50, ErrorMessage = "tooLong")]
        [MinLength(8, ErrorMessage = "tooShort")]
        [DataType(DataType.Password)]
        [Display(Name = "registerPassword")]
        public string Password { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "registerBirthDate")]
        [AgeCheck]
        public DateTime BirthDate { get; set; }

        [DataType(DataType.PhoneNumber)]
        [RegularExpression("^([+])([0-9]{11})$", ErrorMessage = "wrongFormat")]
        [Display(Name = "registerPhoneNumber")]
        public string PhoneNumber { get; set; }


    }

    public class AgeCheck : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = validationContext.ObjectInstance as RegisterViewModel;

            if (model == null)
            {
                throw new ArgumentException("Attribute not applied on registration");
            }

            if (model.BirthDate > DateTime.Now.AddYears(-18))
            {
                return new ValidationResult(GetErrorMessage(validationContext));
            }


            return ValidationResult.Success;
        }

        private string GetErrorMessage(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(this.ErrorMessage))
                return this.ErrorMessage;

            return "Negali būti jaunesnis negu 18 metų";
        }
    }

}
