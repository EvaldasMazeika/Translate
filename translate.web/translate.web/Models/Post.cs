using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace translate.web.Models
{
    public class Post
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public DateTime CreatedTime { get; set; }

        public Guid EmployeeId { get; set; }

        public AppIdentityUser Employee { get; set; }

        public ICollection<Comment> Comments { get; set; }

        public bool IsImportant { get; set; }

    }
}
