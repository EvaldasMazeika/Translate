using System;
using System.ComponentModel.DataAnnotations;

namespace translate.web.Models
{
    public class ProjectMember
    {
        public Guid ProjectId { get; set; }

        public Guid EmployeeId { get; set; }

        public bool AcceptedInvitation { get; set; }
        public bool IsCreator { get; set; }
        public bool ShowOnlyMine { get; set; }

        public DateTime? JoinDate { get; set; }

        public Project Project { get; set; }
        public AppIdentityUser Employee { get; set; }

    }
}