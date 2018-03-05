using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using translate.web.Data;
using translate.web.Models;
using translate.web.ViewModels;

namespace translate.web.Controllers
{
    [Authorize(Roles = "Administrator, Translator")]
    public class HomeController : Controller
    {
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly ApplContext _context;

        public HomeController(UserManager<AppIdentityUser> userManager, ApplContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            var model = _context.Posts.Include(x => x.Employee).ToList();

            return View(model);
        }
        
        [HttpGet]
        public IActionResult Post(Guid id)
        {
            if(String.IsNullOrEmpty(id.ToString()))
            {
              return RedirectToAction("Index");
            }

            var model = _context.Posts.Include(a =>  a.Comments).Where(x => x.Id == id).FirstOrDefault();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Post(NewCommentViewModel model)
        {
            if(ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);

                var result = new Comment
                {
                    CommentText = model.WrittenText,
                    CommentDate = DateTime.Now,
                    PostId = model.PostId,
                    FullName = $"{user.Name} {user.Surname}"
                };

                _context.Add(result);
                await _context.SaveChangesAsync();
                return RedirectToAction("Post");

            }
            return View();
        }

        public IActionResult Account()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if(ModelState.IsValid)
            {
                if(model.NewPassword == model.RepeatPassword)
                {
                    var user = await _userManager.GetUserAsync(HttpContext.User);

                    IdentityResult result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                    if(result.Succeeded)
                    {
                        return RedirectToAction("Account");
                    }

                }
                return View(model);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditSelf(EditMySelfViewModel model)
        {
            if(ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);

                user.Surname = model.Surname;
                user.Email = model.Email;
                user.PhoneNumber = model.MobileNumber;

                IdentityResult result = await _userManager.UpdateAsync(user);
                if(result.Succeeded)
                {
                    return RedirectToAction("Account");
                }

            }

            return View(model);
        }

        public IActionResult Projects()
        {
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> UploadFiles(IFormFile file)
        {
            //TODO: IMPLEMENT NORMAL FILE UPLOAD
            if (file == null || file.Length == 0)
                return Content("file not selected");

            var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot/Uploads",
                        file.FileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return RedirectToAction("Index");

        }

        public async Task<IActionResult> Download(string filename)
        {
            //TODO: IMPLEMENT NORMAL FILE DOWNLOAD
            if (filename == null)
                return Content("filename not present");

            var path = Path.Combine(
                           Directory.GetCurrentDirectory(),
                           "wwwroot/Uploads", filename);

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, GetContentType(path), Path.GetFileName(path));
        }
        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformatsofficedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"},
                {".resx", "text/xml" }
            };
        }

    }
}
