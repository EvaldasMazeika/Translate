using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using translate.web.Data;

namespace translate.web.ViewComponents
{
    [ViewComponent(Name = "PostsIndex")]
    public class PostsIndexViewComponent : ViewComponent
    {
        private readonly ApplContext _context;

        public PostsIndexViewComponent(ApplContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _context.Posts.Include(x => x.Employee).ToListAsync();
            return View(model);
        }
    }
}
