using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace translate.web.Models
{
    public class Project
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreateDate { get; set; }

        public ICollection<ProjectMember> ProjectMembers { get; set; }
        public ICollection<ProjectDocument> ProjectDocuments { get; set; }
    }
}
