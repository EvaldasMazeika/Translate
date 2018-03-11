using System;
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
    [Route("Project/{projectId}")]
    [Authorize(Roles = "Administrator, Translator")]
    public class ProjectController : Controller
    {
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly ApplContext _context;

        public ProjectController(UserManager<AppIdentityUser> userManager, ApplContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index(Guid projectId)
        {
            if (String.IsNullOrEmpty(projectId.ToString()))
            {
                return RedirectToAction("Projects","Home");
            }

            var user = await _userManager.GetUserAsync(HttpContext.User);

            var exist = await _context.ProjectMembers.Where(x => x.Employee == user && x.ProjectId == projectId && x.AcceptedInvitation == true).SingleOrDefaultAsync();

            if (exist == null)
            {
                return RedirectToAction("Projects", "Home");
            }

            var model = await _context.Projects.Where(a => a.Id == projectId).SingleOrDefaultAsync();

            return View(model);
        }

        [HttpPost]
        [Route("AddToProjectAsync")]
        public async Task<IActionResult> AddToProjectAsync([FromBody] AddToProjectViewModel model)
        {
            if(model.Email == null)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            // TODO: CHECK IF IN LIST AND SEND FAILURE

            if (user != null)
            {
                var result = new ProjectMember
                {
                    Employee = user,
                    IsCreator = false,
                    AcceptedInvitation = false,
                    ProjectId = model.ProjectId
                };

                _context.Add(result);
                if (_context.SaveChanges() > 0)
                {
                    return new JsonResult("success");
                }
                else
                {
                    return BadRequest();
                }
            }
            return NotFound();
        }

        [HttpPost]
        [Route("RemoveFromProjectAsync")]
        public async Task<IActionResult> RemoveFromProjectAsync([FromBody] RemoveFromProjectViewModel model)
        {
            if(model.MemberId == null || model.ProjectId == null)
            {
                return BadRequest();
            }

            var result = await _context.ProjectMembers.Where(x => x.ProjectId == model.ProjectId && x.EmployeeId == model.MemberId).SingleOrDefaultAsync();

            if(result == null)
            {
                return BadRequest();
            }

            _context.Remove(result);

            if(_context.SaveChanges() > 0)
            {
                return new JsonResult("success");
            }
            else
            {
                return BadRequest();
            }
        }

        [Route("TeamMembers")]
        public IActionResult TeamMembers(Guid projectId)
        {
            return ViewComponent("ProjectMembers", projectId);
        }
    }
}
