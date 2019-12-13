using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GrozaNews.Models
{
    public class Comment
    {
        public string CommentId { get; set; }
        public string Content { get; set; }

        public string Date { get; set; }
        //de vazut daca treb si newsId pe langa virtual news 
        // public string NewsId {get; set;}
        public virtual News News { get; set; }

        // autor
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}