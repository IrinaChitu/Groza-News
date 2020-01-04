using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace GrozaNews.Models
{
    public class ThumbnailedNews
    {
        [Key]
        public int NewsId { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Headline { get; set; }
        [Required]
        public string LinkStire { get; set; }
        //[Required]
        public byte[] ImageData { get; set; }
        [Required]
        public string Date { get; set; }
        
        public byte[] ImageThumbnail { get; set; }
        public int ImageMimeType { get; internal set; }


        // feature: share pe social media

        //public int CategoryId { get; set; }
        //public virtual Category Category { get; set; }

        //public virtual ICollection<Comment> Comments { get; set; }
        // autor
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; } // rol are si proprietati de profil (e.g. nume nickname)
    }
}