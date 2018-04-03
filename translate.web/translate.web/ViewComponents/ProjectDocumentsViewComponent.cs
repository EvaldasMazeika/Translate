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
    [ViewComponent(Name = "ProjectDocuments")]
    public class ProjectDocumentsViewComponent : ViewComponent
    {
        private readonly ApplContext _context;
        private readonly UserManager<AppIdentityUser> _userManager;

        public ProjectDocumentsViewComponent(ApplContext context, UserManager<AppIdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid ProjectId)
        {
            var user = _userManager.GetUserId(HttpContext.User);
            ViewBag.IsCreator = _context.ProjectMembers.Where(x => x.ProjectId == ProjectId && x.EmployeeId.ToString() == user).Single().IsCreator;

            var temp = await _context.ProjectDocuments.Where(x => x.ProjectId == ProjectId).ToListAsync();

            var model = temp.Select(a => new DocumentsViewModel { Id = a.Id, Name = a.Name, ProjectId = a.ProjectId }).ToList();

            return View(model);
        }
    }
}
