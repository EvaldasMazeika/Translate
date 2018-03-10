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
            if (String.IsNullOrEmpty(id.ToString()))
            {
                return RedirectToAction("Index");
            }

            var model = _context.Posts.Include(a => a.Comments).Where(x => x.Id == id).FirstOrDefault();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Post(NewCommentViewModel model)
        {
            if (ModelState.IsValid)
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
            if (ModelState.IsValid)
            {
                if (model.NewPassword == model.RepeatPassword)
                {
                    var user = await _userManager.GetUserAsync(HttpContext.User);

                    IdentityResult result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                    if (result.Succeeded)
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
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);

                user.Surname = model.Surname;
                user.Email = model.Email;
                user.PhoneNumber = model.MobileNumber;

                IdentityResult result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("Account");
                }

            }

            return View(model);
        }

        public async Task<IActionResult> Projects()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            var model = await _context.ProjectMembers.Where(a => a.Employee == user && a.AcceptedInvitation == true).Include(z => z.Project).ToListAsync();

            return View(model);
        }

        [HttpGet]
        public IActionResult NewProject()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> NewProject(NewProjectViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = new Project
                {
                    Name = model.Name,
                    Description = model.Description,
                    CreateDate = DateTime.Now
                };

                _context.Add(result);

                var user = await _userManager.GetUserAsync(HttpContext.User);

                var creator = new ProjectMember
                {
                    Project = result,
                    JoinDate = DateTime.Now,
                    AcceptedInvitation = true,
                    IsCreator = true,
                    Employee = user
                };

                _context.Add(creator);

                await _context.SaveChangesAsync();
                return RedirectToAction("Projects");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Project(Guid? id)
        {
            if(String.IsNullOrEmpty(id.ToString()))
            {
                return RedirectToAction("Projects");
            }
            var user = await _userManager.GetUserAsync(HttpContext.User);

            var exist = await _context.ProjectMembers.Where(x => x.Employee == user && x.ProjectId == id && x.AcceptedInvitation == true).SingleOrDefaultAsync();

            if(exist == null)
            {
                return RedirectToAction("Projects");
            }

            var model = await _context.Projects.Where(a => a.Id == id).SingleOrDefaultAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddToProject(AddToProjectViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if(user != null)
            {
                var result = new ProjectMember
                {
                    Employee = user,
                    IsCreator = false,
                    AcceptedInvitation = false,
                    ProjectId = model.ProjectId
                };

                _context.Add(result);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Project", new { id = model.ProjectId});
        }

        [HttpPost]
        public async Task<IActionResult> AcceptInvAsync([FromBody] InvitationViewModel data)
        {
            var entity = await _context.ProjectMembers.Where(x => x.ProjectId == data.Pr && x.EmployeeId == data.Emp).SingleOrDefaultAsync();

            entity.AcceptedInvitation = true;
            entity.JoinDate = DateTime.Now;

            _context.Update(entity);

            if(_context.SaveChanges() > 0)
            {
                return Json("Success");
            }
            else
            {
                return Json("Failure");
            }
        }

        public async Task<IActionResult> DeclineInvAsync([FromBody] InvitationViewModel data)
        {
            var entity = await _context.ProjectMembers.Where(x => x.ProjectId == data.Pr && x.EmployeeId == data.Emp).SingleOrDefaultAsync();

            _context.Remove(entity);

            if (_context.SaveChanges() > 0)
            {
                return Json("Success");
            }
            else
            {
                return Json("Failure");
            }
        }


        public IActionResult Invitations()
        {
            return ViewComponent("InvitationsIndex");
        }

        public IActionResult ProjectsAsync()
        {
            return ViewComponent("ProjectsIndex");
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
