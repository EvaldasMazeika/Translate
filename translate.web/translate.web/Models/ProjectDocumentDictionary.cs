using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace translate.web.Models
{
    public class ProjectDocumentDictionary
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }


        public ProjectDocument Document { get; set; }
    }
}
