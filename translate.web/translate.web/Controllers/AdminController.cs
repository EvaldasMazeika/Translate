using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        public IActionResult Posts()
        {
            var model = _context.Posts.ToList();

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
            if(ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);

                var post = new Post
                {
                    Title = model.Title,
                    Message = model.Message,
                    CreatedTime = DateTime.Now,
                    Employee = user
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
                return RedirectToAction("Posts");
            }
            return RedirectToAction("Posts");
        }


    }
}
