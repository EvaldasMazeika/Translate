using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using translate.web.Data;
using translate.web.Models;

namespace translate.web.ViewComponents
{
    [ViewComponent(Name = "EditorPanel")]
    public class EditorPanelViewComponent : ViewComponent
    {
        private readonly ApplContext _context;

        public EditorPanelViewComponent(ApplContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(TranslationDictionary dictionary)
        {
            var model = await _context.TranslationDictionarys.Where(x => x.Id == dictionary.Id)
                .Include(a => a.Translations)
                .ThenInclude(a => a.Document).SingleOrDefaultAsync();

            return View(model);
        }
    }
}
