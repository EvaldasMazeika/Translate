using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace translate.web.ViewModels
{
    public class CreateNewWordViewModel
    {
        public Guid TranslationId { get; set; }
        public string KeyValue { get; set; }
        public string ValueValue { get; set; }
    }
}
