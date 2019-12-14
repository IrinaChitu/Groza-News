using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GrozaNews.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public string Date { get; set; }
        //de vazut daca treb si newsId pe langa virtual news 
        public string NewsId {get; set;}
        public virtual News News { get; set; }

        // autor
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}