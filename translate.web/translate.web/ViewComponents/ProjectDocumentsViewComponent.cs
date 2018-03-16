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
    [ViewComponent(Name = "ProjectDocuments")]
    public class ProjectDocumentsViewComponent : ViewComponent
    {
        private readonly ApplContext _context;

        public ProjectDocumentsViewComponent(ApplContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid ProjectId)
        {
            var temp = await _context.ProjectDocuments.Where(x => x.ProjectId == ProjectId).ToListAsync();

            var model = temp.Select(a => new DocumentsViewModel { Id = a.Id, Name = a.Name, IsLoaded = a.IsLoaded }).ToList();

            return View(model);
        }
    }
}
