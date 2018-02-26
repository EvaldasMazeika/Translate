using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace translate.web.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplContext context)
        {
            context.Database.EnsureCreated();



        }
    }
}
