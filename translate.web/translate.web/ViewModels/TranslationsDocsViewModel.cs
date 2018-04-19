using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace translate.web.ViewModels
{
    public class TranslationsDocsViewModel
    {
        public int OpenTranslations { get; set; }
        public int ClosedTtranslations { get; set; }
        public int WaitingTranslations { get; set; }
    }
}
