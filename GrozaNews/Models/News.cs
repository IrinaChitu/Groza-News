using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GrozaNews.Models
{
    public class News
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Date { get; set; }

        // feature: share pe social media

        public virtual Category Category { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
        // autor
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; } // rol are si proprietati de profil (e.g. nume nickname)
    }
}