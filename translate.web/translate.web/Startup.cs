using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using translate.web.Data;
using translate.web.Models;
using translate.web.Resources;
using translate.web.Services;

namespace translate.web
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            _configuration = configuration;

            var builder = new ConfigurationBuilder();
            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }
            else
            {
                builder.AddEnvironmentVariables();
            }
            _configuration = builder.Build();

        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplContext>(opt => opt.UseSqlServer(_configuration["DefaultConnection"]));

            services.AddIdentity<AppIdentityUser, AppIdentityRole>()
                .AddEntityFrameworkStores<ApplContext>()
                .AddDefaultTokenProviders();
            //@"C:\Users\Evaldas\Desktop\uploads"
            services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory())));


            services.AddTransient<IEmailSender, EmailSender>();

            services.AddSingleton<LocService>();
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.Configure<Emailoptions>(_configuration);

            services.AddMvc()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization(options =>
                {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                    {
                        var assemblyName = new AssemblyName(typeof(SharedResource).GetTypeInfo().Assembly.FullName);
                        return factory.Create("SharedResource", assemblyName.Name);
                    };
                });

            services.Configure<RequestLocalizationOptions>(
                options =>
                {
                    var supportedCultures = new List<CultureInfo>
                        {
                    new CultureInfo("en-US"),
                    new CultureInfo("lt")
                        };

                    options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US");
                    options.SupportedCultures = supportedCultures;
                    options.SupportedUICultures = supportedCultures;

                    options.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider());
                });



            services.AddAuthentication().AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = _configuration["Authentication:Google:ClientId"];
                googleOptions.ClientSecret = _configuration["Authentication:Google:ClientSecret"];
            });


            services.Configure<IdentityOptions>(opt => 
            {
                opt.Lockout.MaxFailedAccessAttempts = 5;
                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                opt.Password.RequireDigit = true;
                opt.Password.RequireUppercase = true;
                opt.Password.RequiredLength = 8;
                opt.Password.RequireLowercase = true;
                opt.Password.RequireNonAlphanumeric = false;
            });

            services.ConfigureApplicationCookie(options => 
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(120);
                options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                options.Cookie = new CookieBuilder
                {
                    HttpOnly = true,
                    Name = ".App.Sec.Cookie",
                    Path = "/",
                    SameSite = SameSiteMode.Lax,
                    SecurePolicy = CookieSecurePolicy.SameAsRequest
                };
            });

            //services.Configure<MvcOptions>(options =>
            //{
            //    options.Filters.Add(new RequireHttpsAttribute());
            //});


            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, ApplContext context, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();


            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

           // var optionss = new RewriteOptions().AddRedirectToHttps();

           // app.UseRewriter(optionss);

            var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(locOptions.Value);


            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(options => 
            {
                options.MapRoute(
                     name: "default",
                     template: "{controller=Home}/{action=Index}/{id?}");
            });

            DbInitializer.Initialize(context);

        }
    }
}
