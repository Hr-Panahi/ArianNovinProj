using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArianNovinWeb.Models
{
    public class Enrollment
    {
        [Key]
        public int EnrollmentId { get; set; }

        [Required]
        [ForeignKey("Course")]
        public int CourseId { get; set; }
        public Course Course { get; set; } // Navigation property for the course

        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; }
        public IdentityUser User { get; set; } // Navigation property for the user

        [Required]
        public DateTime EnrolledAt { get; set; } = DateTime.Now;
    }
}