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
    [ViewComponent(Name = "WordsList")]
    public class WordsListViewComponent : ViewComponent
    {
        private readonly ApplContext _context;

        public WordsListViewComponent(ApplContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync(EditorListViewModel model)
        {
            var stat = 10;

            var totalCount = await _context.TranslationDictionarys.Where(x => x.TranslationId == model.Id).CountAsync();
            var toTranslate = await _context.TranslationDictionarys.Where(x => x.TranslationId == model.Id && x.NewValue == null).CountAsync();

            var result = await _context.TranslationDictionarys.Where(x => x.TranslationId == model.Id)
                .Include(a=>a.Translations)
                /*.ThenInclude(q=>q.Document)*/.Skip((model.PageIndex -1) * stat).Take(stat).ToListAsync();

            ViewBag.trans = model.Id;

            ViewBag.pageIndex = model.PageIndex;

            ViewBag.previous = model.PageIndex == 1 ? false : true;
            ViewBag.next = totalCount > model.PageIndex * stat ? true : false;
            ViewBag.toTranslate = toTranslate;

            return View(result);
        }
    }
}
