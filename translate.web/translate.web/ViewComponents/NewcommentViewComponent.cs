using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using translate.web.Models;
using translate.web.ViewModels;

namespace translate.web.ViewComponents
{
    [ViewComponent(Name = "Newcomment")]
    public class NewcommentViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Guid topicId)
        {
            var model = new NewCommentViewModel { PostId = topicId };

            return View(model);
        }
    }
}
