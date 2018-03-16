using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace translate.web.Models
{
    public class Translation
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public Guid LanguageId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }

        public ProjectDocument Document { get; set; }
        public Language Language { get; set; }
        public ICollection<TranslationDictionary> TranslationDictionarys { get; set; }

    }
}
