using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace translate.web.ViewModels
{
    public class UploadFileViewModel
    {
        public IFormFile File { get; set; }
        public Guid LanguageId { get; set; }

    }
}
