using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace translate.web.ViewModels
{
    public class NewCommentViewModel
    {
        public Guid PostId { get; set; }
        public string WrittenText { get; set; }

    }
}
