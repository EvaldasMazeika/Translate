using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace translate.web.ViewModels
{
    public class EmailConfirmViewModel
    {
        [Required]
        [Display(Name = "Patvirtinimo kodas")]
        public string Code { get; set; }
    }
}
