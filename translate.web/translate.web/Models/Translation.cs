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
        public Guid LanguageId { get; set; }
        public string Title { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsWaiting { get; set; }
        public string DeclineComment { get; set; }
        public Guid TranslatorId { get; set; }
        public DateTime AddedDate { get; set; }
        public bool HasDocument { get; set; }
        public Guid ProjectId { get; set; }

        public Project Project { get; set; }
        public Language Language { get; set; }
        public AppIdentityUser Translator { get; set; }
        public ICollection<TranslationDictionary> TranslationDictionarys { get; set; }

    }
}
