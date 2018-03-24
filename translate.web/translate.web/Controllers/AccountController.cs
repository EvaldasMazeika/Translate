using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using translate.web.Models;
using translate.web.Resources;
using translate.web.ViewModels;

namespace translate.web.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<AppIdentityUser> _signInManager;
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly RoleManager<AppIdentityRole> _roleManager;
        private readonly LocService _loc;

        public AccountController(SignInManager<AppIdentityUser> signInManager,
            UserManager<AppIdentityUser> userManager,
            RoleManager<AppIdentityRole> roleManager,
            LocService loc)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _loc = loc;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if(ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);
                if(result.Succeeded)
                {
                    return RedirectToLocal(returnUrl);
                }
            }

            ModelState.AddModelError(string.Empty, _loc.GetLocalizedHtmlString("loginError"));
            return View(model); 
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if(ModelState.IsValid)
            {
                var user = new AppIdentityUser
                {
                    UserName = model.UserName,
                    Name = model.Name,
                    Surname = model.Surname,
                    Email = model.Email,
                    BirthDate = model.BirthDate,
                    PhoneNumber = model.PhoneNumber
                };

                IdentityResult result = await _userManager.CreateAsync(user, model.Password);
                if(result.Succeeded)
                {
                    if(_userManager.Users.Count() == 1) // jei zmogus pirmas, skiriam jam admino teises (kazkodel, kaip tuscia rodo 1)
                    {
                        AppIdentityRole role = await _roleManager.FindByNameAsync("Administrator");
                        if(role != null)
                        {
                            IdentityResult roleResult = await _userManager.AddToRoleAsync(user,role.Name);
                            if(roleResult.Succeeded)
                            {
                                return RedirectToAction("Login");
                            }
                        }
                    }
                    else
                    {
                        AppIdentityRole role = await _roleManager.FindByNameAsync("Translator");
                        if (role != null)
                        {
                            IdentityResult roleResult = await _userManager.AddToRoleAsync(user, role.Name);
                            if (roleResult.Succeeded)
                            {
                                return RedirectToAction("Login");
                            }
                        }
                    }
                }
            }
            return View(model);
        }


        private IActionResult RedirectToLocal(string returnUrl)
        {
            if(Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var user = new AppIdentityUser { UserName = email, Email = email };
            var createResult = await _userManager.CreateAsync(user);

            if (createResult.Succeeded)
            {
                AppIdentityRole role = await _roleManager.FindByNameAsync("Translator");
                IdentityResult roleResult = await _userManager.AddToRoleAsync(user, role.Name);
                if (roleResult.Succeeded)
                {
                    createResult = await _userManager.AddLoginAsync(user, info);
                    if (createResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToLocal(returnUrl);
                    }
                }

            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }


        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}
