using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace translate.web.ViewModels
{
    public class ForumPostsViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public bool IsImportant { get; set; }
        public DateTime CreatedTime { get; set; }
        public string CreatorName { get; set; }
        public int CommentsCount { get; set; }
        public string LastCommentAuthor { get; set; }
    }
}
