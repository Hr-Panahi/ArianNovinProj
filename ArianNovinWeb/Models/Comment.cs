﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArianNovinWeb.Models
{
    public class Comment
    {
        [Key]
        public int? CommentId { get; set; }

        [Required]
        public string? Content { get; set; }

        [Required]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        [Required]
        [ForeignKey("Author")]
        public string? AuthorId { get; set; }
        public IdentityUser? Author { get; set; }  // navigation property

        [ForeignKey("Post")]
        public int? PostId { get; set; }
        public Post? Post { get; set; }

        [ForeignKey("ParentComment")]
        public int? ParentCommentId { get; set; }
        public Comment? ParentComment { get; set; } // Self-referencing foreign key for nested comments (Replies)

        public ICollection<Comment> Replies { get; set; } = new List<Comment>(); //navigation property for child comments
    }
}
