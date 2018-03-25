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
            ViewBag.creator = _context.ProjectMembers.Where(x => x.ProjectId == ProjectId && x.EmployeeId.ToString() == user).Single().IsCreator;

            var model = await _context.Translations.Where(x => x.Document.ProjectId == ProjectId)
                .Include(a => a.Document)
                .Include(a=>a.TranslationDictionarys)
                .Include(i=> i.Translator)
                .ToListAsync();

            return View(model);
        }
    }
}
