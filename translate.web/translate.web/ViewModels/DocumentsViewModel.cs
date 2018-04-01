﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace translate.web.ViewModels
{
    public class DocumentsViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsLoaded { get; set; }
        public Guid ProjectId { get; set; }
    }
}
