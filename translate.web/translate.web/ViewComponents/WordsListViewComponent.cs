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
    [ViewComponent(Name = "WordsList")]
    public class WordsListViewComponent : ViewComponent
    {
        private readonly ApplContext _context;

        public WordsListViewComponent(ApplContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync(Guid TranslationId)
        {
            var model = await _context.TranslationDictionarys.Where(x => x.TranslationId == TranslationId && x.NewValue == null)
                .Include(a=>a.Translations)
                .ThenInclude(q=>q.Document).ToListAsync();

            return View(model);
        }
    }
}
