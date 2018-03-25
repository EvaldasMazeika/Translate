using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using translate.web.ViewModels;

namespace translate.web.ViewComponents
{
    [ViewComponent(Name = "AddToProject")]
    public class AddToProjectViewComponent : ViewComponent
    {

        public IViewComponentResult Invoke(Guid ProjectId)
        {
            var model = new AddToProjectViewModel
            {
                ProjectId = ProjectId
            };

            return View(model);
        }
    }
}
