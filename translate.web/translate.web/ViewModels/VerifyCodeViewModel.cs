﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace translate.web.ViewModels
{
    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Gautas kodas")]
        public string Code { get; set; }

        public string ReturnUrl { get; set; }

        [Display(Name = "Įsiminti naršyklę?")]
        public bool RememberBrowser { get; set; }

        [Display(Name = "Prisiminti mane?")]
        public bool RememberMe { get; set; }
    }
}
