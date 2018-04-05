using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using translate.web.Models;
using translate.web.Resources;
using translate.web.Services;
using translate.web.ViewModels;

namespace translate.web.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<AppIdentityUser> _signInManager;
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly RoleManager<AppIdentityRole> _roleManager;
        private readonly LocService _loc;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public AccountController(SignInManager<AppIdentityUser> signInManager,
            UserManager<AppIdentityUser> userManager,
            RoleManager<AppIdentityRole> roleManager,
            LocService loc,
            IEmailSender emailSender,
            IConfiguration configuration)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _loc = loc;
            _emailSender = emailSender;
            _configuration = configuration;
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
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: true);
                if(result.Succeeded)
                {
                    return RedirectToLocal(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction(nameof(LoginWith2fa), new { rememberMe = model.RememberMe, returnUrl = returnUrl });
                }
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Paskyra trumpam užrakinta dėl klaidingų prisijungimų");
                }

            }

            ModelState.AddModelError(string.Empty, _loc.GetLocalizedHtmlString("loginError"));
            return View(model); 
        }

        [HttpGet]
        public async Task<IActionResult> LoginWith2fa(bool rememberMe, string returnUrl = null)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException("Error.");
            }
            var code = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
            if (string.IsNullOrWhiteSpace(code))
            {
                return View("Error");
            }
            var message = $"Jūsų prisijungimo kodas: {code}";
            await _emailSender.SendEmailAsync(await _userManager.GetEmailAsync(user), "Prisijungimas", message);

            var model = new VerifyCodeViewModel { ReturnUrl = returnUrl, RememberMe = rememberMe, Provider = "Email" };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2fa(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.TwoFactorSignInAsync(model.Provider, model.Code, model.RememberMe, model.RememberBrowser);
            if (result.Succeeded)
            {
                return RedirectToLocal(model.ReturnUrl);
            }
            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "užrakinta.");
                return View(model);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Blogas kodas.");
                return View(model);
            }
        }

        public IActionResult Register()
        {
            ViewData["ReCaptchaKey"] = _configuration.GetSection("GoogleReCaptcha:key").Value;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            ViewData["ReCaptchaKey"] = _configuration.GetSection("GoogleReCaptcha:key").Value;

            if (ModelState.IsValid)
            {
                if (!ReCaptchaPassed(Request.Form["g-recaptcha-response"], _configuration.GetSection("GoogleReCaptcha:secret").Value))
                {
                    ModelState.AddModelError(string.Empty, "Būtina patvirtirtinti CAPTCHA");
                    return View(model);
                }

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

        public static bool ReCaptchaPassed(string gRecaptchaResponse, string secret)
        {
            HttpClient httpClient = new HttpClient();
            var res = httpClient.GetAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={gRecaptchaResponse}").Result;
            if (res.StatusCode != HttpStatusCode.OK)
            {
                return false;
            }

            string JSONres = res.Content.ReadAsStringAsync().Result;
            dynamic JSONdata = JObject.Parse(JSONres);
            if (JSONdata.success != "true")
            {
                return false;
            }

            return true;
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

        [HttpPost]
        public async Task<IActionResult> MyAccount()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            await _signInManager.SignOutAsync();

            await _userManager.DeleteAsync(user);

            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}
