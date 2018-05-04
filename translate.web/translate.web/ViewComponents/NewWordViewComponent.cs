using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using translate.web.ViewModels;

namespace translate.web.ViewComponents
{
    [ViewComponent(Name = "NewWord")]
    public class NewWordViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(InsertWordViewModel model)
        {

            return View(model);
        }
    }
}
