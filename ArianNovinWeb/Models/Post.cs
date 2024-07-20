using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ArianNovinWeb.Models
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Image { get; set; }

        [Required]
        public DateTime CreateDate { get; set; } = DateTime.Now;

        [ForeignKey("User")]
        public int AuthorId { get; set; }
        public User Author { get; set; } // Navigation property
    }
}
