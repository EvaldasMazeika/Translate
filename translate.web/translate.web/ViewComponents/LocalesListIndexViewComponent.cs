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
    [ViewComponent(Name = "LocalesListIndex")]
    public class LocalesListIndexViewComponent : ViewComponent
    {
        private readonly ApplContext _context;
        private readonly UserManager<AppIdentityUser> _userManager;

        public LocalesListIndexViewComponent(ApplContext context, UserManager<AppIdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IViewComponentResult> InvokeAsync(Guid ProjectId)
        {
            var user = _userManager.GetUserId(HttpContext.User);
            ViewBag.userId = user;

            var projectMember = await _context.ProjectMembers.Where(w => w.ProjectId == ProjectId && w.EmployeeId.ToString() == user).FirstOrDefaultAsync();
            ViewBag.creator = projectMember.IsCreator;
            ViewBag.showAll = projectMember.ShowOnlyMine;

            List<Translation> model = null;

            if (projectMember.ShowOnlyMine == false)
            {

                model = await _context.Translations.Where(x => x.ProjectId == ProjectId)
                   .Include(a => a.TranslationDictionarys)
                   .Include(i => i.Translator)
                   .Include(i => i.Language)
                   .OrderByDescending(o => o.AddedDate)
                   .ToListAsync();
            }
            else
            {
                model = await _context.Translations.Where(w => w.ProjectId == ProjectId && w.TranslatorId.ToString() == user)
                   .Include(a => a.TranslationDictionarys)
                   .Include(i => i.Translator)
                   .Include(i => i.Language)
                   .OrderByDescending(o => o.AddedDate)
                   .ToListAsync();
            }
            ViewBag.docLanguage = _context.ProjectDocuments.Where(w => w.ProjectId == ProjectId).SingleOrDefault().Language.Name;
            return View(model);
        }
    }
}
