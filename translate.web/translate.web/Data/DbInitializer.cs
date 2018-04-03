using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using translate.web.Models;

namespace translate.web.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplContext context)
        {
            //context.Database.EnsureCreated();

            //var a1 = new Language { Name = "English - en" };
            //context.Add(a1);
            //var a2 = new Language { Name = "Lithuanian - lt" };
            //context.Add(a2);
            //context.SaveChanges();

            if (context.Roles.Any())
            {
                return;
            }

            AppIdentityRole adminRole = new AppIdentityRole
            {
                DateCreated = DateTime.Now,
                Name = "Administrator",
                Description = "site role"
            };
            context.Roles.Add(adminRole);

            AppIdentityRole translateRole = new AppIdentityRole
            {
                DateCreated = DateTime.Now,
                Name = "Translator",
                Description = "translator role"
            };
            context.Roles.Add(translateRole);

            context.SaveChanges();
        }
    }
}
