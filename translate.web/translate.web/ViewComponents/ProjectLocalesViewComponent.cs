using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using translate.web.Data;

namespace translate.web.ViewComponents
{
    [ViewComponent(Name ="ProjectLocales")]
    public class ProjectLocalesViewComponent : ViewComponent
    {
        private readonly ApplContext _context;

        public ProjectLocalesViewComponent(ApplContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync(Guid ProjectId)
        {
            var s = await _context.ProjectDocuments.Where(x => x.ProjectId == ProjectId && x.IsLoaded == true).FirstOrDefaultAsync();
            if (s == null)
            {
                ViewBag.exist = false;
            }
            else
            {
                ViewBag.exist = true;
            }

            var model = await _context.Translations.Where(x => x.Document.ProjectId == ProjectId)
                .Include(i => i.TranslationDictionarys)
                .ToListAsync();

            return View(model);
        }
    }
}
