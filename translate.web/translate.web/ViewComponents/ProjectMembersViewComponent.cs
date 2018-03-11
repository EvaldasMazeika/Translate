using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using translate.web.Data;
using translate.web.ViewModels;

namespace translate.web.ViewComponents
{
    [ViewComponent(Name = "ProjectMembers")]
    public class ProjectMembersViewComponent : ViewComponent
    {
        private readonly ApplContext _context;

        public ProjectMembersViewComponent(ApplContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid ProjectId)
        {
           var members = await _context.ProjectMembers.Include(a => a.Employee).Where(x => x.ProjectId == ProjectId).ToListAsync();

            var model = new AddToProjectViewModel
            {
                ProjectId = ProjectId,
                Members = members.Select(x => x.Employee)
            };

            return View(model);
        }
    }
}
