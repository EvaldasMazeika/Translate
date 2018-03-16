using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace translate.web.Models
{
    public class TranslationDictionary
    {
        public Guid Id { get; set; }
        public Guid TranslationId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        public Translation Translations { get; set; }
    }
}
