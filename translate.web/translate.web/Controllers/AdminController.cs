﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using translate.web.Data;
using translate.web.Models;
using translate.web.ViewModels;

namespace translate.web.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly ApplContext _context;
        private readonly UserManager<AppIdentityUser> _userManager;

        public AdminController(ApplContext context, UserManager<AppIdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Posts()
        {
            var model = await _context.Posts.ToListAsync();

            return View(model);
        }

        [HttpGet]
        public IActionResult NewPost()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> NewPost(NewPostViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);

                var post = new Post
                {
                    Title = model.Title,
                    Message = model.Message,
                    CreatedTime = DateTime.Now,
                    Employee = user,
                    IsImportant = model.IsImportant
                };
                _context.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction("Posts");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeletePost(Guid id)
        {
            if (!String.IsNullOrEmpty(id.ToString()))
            {
                var result = _context.Posts.Where(x => x.Id == id).SingleOrDefault();

                _context.Remove(result);
                await _context.SaveChangesAsync();
                return new JsonResult("success");
            }
            return BadRequest();
        }

        public IActionResult Languages()
        {
            var model = _context.Languages.ToList();

            return View(model);
        }

        [HttpGet]
        public IActionResult NewLanguage()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> NewLanguage(NewLanguageViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = new Language { Name = model.Name, Code = model.Code };
                _context.Add(result);
                await _context.SaveChangesAsync();
                return RedirectToAction("Languages");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLanguage([FromBody] Language language)
        {
            var lang = await _context.Languages.Where(w => w.Id == language.Id).SingleOrDefaultAsync();
            if (lang != null)
            {
                _context.Remove(lang);
                await _context.SaveChangesAsync();
                return new JsonResult("success");
            }

            return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> Documents()
        {
            var model = await _context.DocumentTypes.ToListAsync();

            return View(model);
        }

        [HttpGet]
        public IActionResult NewDocument()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> NewDocument(NewDocumentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = new DocumentType { Name = model.Name, Example = model.Example };
            _context.Add(result);
            await _context.SaveChangesAsync();

            return RedirectToAction("Documents");
        }

    }
}
