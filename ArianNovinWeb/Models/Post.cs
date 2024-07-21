using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public string ImagePath { get; set; }

        [Required]
        public DateTime CreateDate { get; set; } = DateTime.Now;
        [Required]
        [ForeignKey("Author")]
        public string? AuthorId { get; set; }
        public IdentityUser? Author { get; set; } //navigation property
        public ICollection<Comment>? Comments { get; set; } = new List<Comment>(); // Navigation property for comments

    }
}
