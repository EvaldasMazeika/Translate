﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using translate.web.Data;
using translate.web.Models;

namespace translate.web.ViewComponents
{
    [ViewComponent(Name = "EditorPanel")]
    public class EditorPanelViewComponent : ViewComponent
    {
        private readonly ApplContext _context;
        private readonly IConfiguration _configuration;

        public EditorPanelViewComponent(ApplContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IViewComponentResult> InvokeAsync(TranslationDictionary dictionary)
        {
            ViewBag.yandexKey = _configuration["YandexApiKey"];

            var projectId = _context.Translations.Where(w => w.Id == dictionary.TranslationId).SingleOrDefault().ProjectId;

            ViewBag.languageId = _context.ProjectDocuments
                .Where(w => w.ProjectId == projectId)
                    .Include(i => i.Language)
                    .SingleOrDefault().Language.Code;

            var model = await _context.TranslationDictionarys.Where(x => x.Id == dictionary.Id)
                .Include(a => a.Translations)
                    .ThenInclude(t=>t.Language)
                //    .Include(a => a.Translations)
                //    .ThenInclude(a => a.Document)
                //        .ThenInclude(r=>r.Language)
                    .SingleOrDefaultAsync();

            return View(model);
        }
    }
}
