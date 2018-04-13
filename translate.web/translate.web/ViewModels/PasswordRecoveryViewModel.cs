using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace translate.web.ViewModels
{
    public class PasswordRecoveryViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "El. paštas")]
        public string Email { get; set; }
    }
}
