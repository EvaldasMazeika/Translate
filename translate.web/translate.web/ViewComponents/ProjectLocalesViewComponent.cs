using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace translate.web.ViewComponents
{
    [ViewComponent(Name ="ProjectLocales")]
    public class ProjectLocalesViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(Guid ProjectId)
        {
            return View();
        }
    }
}
