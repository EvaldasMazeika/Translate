using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using translate.web.Models;

namespace translate.web.ViewModels
{
    public class AddToProjectViewModel
    {
        public Guid ProjectId { get; set; }

        [Display(Name ="emailAddress")]
        public string Email { get; set; }

        public IEnumerable<AppIdentityUser> Members { get; set; }

    }
}
