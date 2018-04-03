using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace translate.web.Models
{
    public class ProjectDocument
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Header { get; set; }

        public Guid ProjectId { get; set; }

        public Guid LanguageId { get; set; }

        public Project Project { get; set; }

        public Language Language { get; set; }

        public ICollection<ProjectDocumentDictionary> ProjectDocumentDictionarys { get; set; }
        public ICollection<Translation> Translations { get; set; }
    }
}
