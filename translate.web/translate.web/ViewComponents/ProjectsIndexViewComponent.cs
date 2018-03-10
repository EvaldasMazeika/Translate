using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using translate.web.Data;
using translate.web.Models;

namespace translate.web.ViewComponents
{
    [ViewComponent(Name = "ProjectsIndex")]
    public class ProjectsIndexViewComponent : ViewComponent
    {
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly ApplContext _context;

        public ProjectsIndexViewComponent(UserManager<AppIdentityUser> userManager, ApplContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var model = await _context.ProjectMembers.Where(a => a.Employee == user && a.AcceptedInvitation == true).Include(z => z.Project).ToListAsync();

            return View(model);
        }
    }
}
