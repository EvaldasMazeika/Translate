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
            context.Database.EnsureCreated();

            if(context.Roles.Any())
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
