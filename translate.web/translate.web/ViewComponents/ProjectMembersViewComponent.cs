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
using translate.web.ViewModels;

namespace translate.web.ViewComponents
{
    [ViewComponent(Name = "ProjectMembers")]
    public class ProjectMembersViewComponent : ViewComponent
    {
        private readonly ApplContext _context;
        private readonly UserManager<AppIdentityUser> _userManager;

        public ProjectMembersViewComponent(ApplContext context, UserManager<AppIdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid ProjectId)
        {
            var user = _userManager.GetUserId(HttpContext.User);
            ViewBag.userId = user;
            ViewBag.creator = _context.ProjectMembers.Where(x => x.ProjectId == ProjectId && x.EmployeeId.ToString() == user).Single().IsCreator;
            ViewBag.project = ProjectId;

            var members = await _context.ProjectMembers
                .Include(a => a.Employee)
                .Where(x => x.ProjectId == ProjectId && x.AcceptedInvitation == true)
                .OrderByDescending(o=> o.JoinDate)
                .Skip(0).Take(5)
                .ToListAsync();

            var model = new AddToProjectViewModel
            {
                ProjectId = ProjectId,
                Members = members.Select(x => x.Employee)
            };

            return View(model);
        }
    }
}
