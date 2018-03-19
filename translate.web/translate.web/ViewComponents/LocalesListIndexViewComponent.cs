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
    [ViewComponent(Name = "LocalesListIndex")]
    public class LocalesListIndexViewComponent : ViewComponent
    {
        private readonly ApplContext _context;

        public LocalesListIndexViewComponent(ApplContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync(Guid ProjectId)
        {
            var model = await _context.Translations.Where(x => x.Document.ProjectId == ProjectId)
                .Include(a => a.Document)
                .Include(a=>a.TranslationDictionarys)
                .ToListAsync();

            return View(model);
        }
    }
}
