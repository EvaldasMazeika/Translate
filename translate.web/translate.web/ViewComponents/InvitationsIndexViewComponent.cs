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
    [ViewComponent(Name = "InvitationsIndex")]
    public class InvitationsIndexViewComponent : ViewComponent
    {
        private readonly ApplContext _context;
        private readonly UserManager<AppIdentityUser> _userManager;

        public InvitationsIndexViewComponent(ApplContext context, UserManager<AppIdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            var model = _context.ProjectMembers.Where(x => x.Employee == user && x.AcceptedInvitation == false).Include(a => a.Project).ToList();

            return View(model);
        }
    }
}
