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
using translate.web.Resources;
using translate.web.Services;
using translate.web.ViewModels;

namespace translate.web.Controllers
{
    [Authorize(Roles = "Administrator, Translator")]
    public class HomeController : Controller
    {
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly ApplContext _context;
        private readonly IEmailSender _emailSender;
        private readonly LocService _locService;

        public HomeController(UserManager<AppIdentityUser> userManager, ApplContext context, IEmailSender emailSender, LocService locService)
        {
            _userManager = userManager;
            _context = context;
            _emailSender = emailSender;
            _locService = locService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Forum()
        {
            var model = _context.Posts.Include(x => x.Employee).Include(i=>i.Comments).ToList();
            var result = new List<ForumPostsViewModel>();

            foreach (var item in model)
            {
                var last = item.Comments.OrderBy(o => o.CommentDate).LastOrDefault();
                var commentAuthor = last != null ? $"{last.FullName} {last.CommentDate}" : "";

                var temp = new ForumPostsViewModel()
                {
                    Id = item.Id,
                    CreatedTime = item.CreatedTime,
                    Title = item.Title,
                    IsImportant = item.IsImportant,
                    CreatorName = $"{item.Employee.Name} {item.Employee.Surname}",
                    CommentsCount = item.Comments.Count,
                    LastCommentAuthor = $"{commentAuthor}"
                };

                result.Add(temp);
            }
            

            return View(result.OrderByDescending(o=>o.CreatedTime));
        }


        [HttpGet]
        public IActionResult Post(Guid id)
        {
            if (String.IsNullOrEmpty(id.ToString()))
            {
                return RedirectToAction("Index");
            }

            var model = _context.Posts.Where(x => x.Id == id)
                .Include(a => a.Comments)
                .Include(i=>i.Employee)
                .FirstOrDefault();

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

        [HttpGet]
        public async Task<IActionResult> MyProfile()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            EditMySelfViewModel model = new EditMySelfViewModel
            {
                Email = user.Email,
                Surname = user.Surname,
                MobileNumber = user.PhoneNumber,
                FirstName = user.Name
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> MyProfile(EditMySelfViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);

                if (model.Email != user.Email)
                {
                    await _userManager.SetTwoFactorEnabledAsync(user, false);
                    user.EmailConfirmed = false;

                    user.Surname = model.Surname;
                    user.PhoneNumber = model.MobileNumber;
                    user.Name = model.FirstName;
                    user.Email = model.Email;
                }
                else
                {
                    user.Surname = model.Surname;
                    user.PhoneNumber = model.MobileNumber;
                    user.Name = model.FirstName;
                }



                IdentityResult result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["message"] = "success";
                    return RedirectToAction("MyProfile");
                }

            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> MyCreds()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var hasPass = await _userManager.HasPasswordAsync(user);

            var model = new ChangePasswordViewModel { HasPassword = hasPass };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> MyCreds(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                var hasPassword = await _userManager.HasPasswordAsync(user);
                if (!hasPassword)
                {
                    var newResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
                    if (newResult.Succeeded)
                    {
                        TempData["message"] = "success";
                        return RedirectToAction("MyCreds");
                    }
                }
                else
                {
                    var isCorrect = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
                    if (isCorrect && model.NewPassword == model.RepeatPassword)
                    {
                        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                        if (result.Succeeded)
                        {
                            TempData["message"] = "success";
                            return RedirectToAction("MyCreds");
                        }
                    }
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

        [HttpPost]
        public async Task<IActionResult> DeleteProject([FromBody] Project model)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var IsCreator = _context.ProjectMembers.Where(x => x.EmployeeId == user.Id && x.ProjectId == model.Id).Single().IsCreator;

            if (IsCreator)
            {
                var project = await _context.Projects.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
                _context.Remove(project);
                await _context.SaveChangesAsync();
                return new JsonResult("success");
            }
            else
            {
                return BadRequest();
            }
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

        [HttpGet]
        public IActionResult MyAccount()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> MySecurity()
        {
            var model = await _userManager.GetUserAsync(HttpContext.User);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var message = $"{_locService.GetLocalizedHtmlString("emailCodeTextToSend")}: {code}";
            await _emailSender.SendEmailAsync(user.Email, $"{_locService.GetLocalizedHtmlString("emailSubjectConfirmation")}", message);

            return RedirectToAction("EmailCode");

        }

        [HttpGet]
        public IActionResult EmailCode()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmailCode(EmailConfirmViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.GetUserAsync(HttpContext.User);

            var result = await _userManager.ConfirmEmailAsync(user, model.Code);
            if (result.Succeeded)
            {
                return RedirectToAction("MySecurity");
            }
            ModelState.AddModelError(string.Empty, $"{_locService.GetLocalizedHtmlString("genericError")}");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EnableTwoFactorAuthentication()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user != null)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, true);
            }
            return RedirectToAction("MySecurity");
        }

        [HttpPost]
        public async Task<IActionResult> DisableTwoFactorAuthentication()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user != null)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, false);
            }
            return RedirectToAction("MySecurity");
        }


        public IActionResult Invitations()
        {
            return ViewComponent("InvitationsIndex");
        }

        public IActionResult ProjectsDropdown()
        {
            return PartialView("_ProjectsDropdownPartial");
        }

    }
}
