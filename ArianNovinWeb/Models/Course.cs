using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArianNovinWeb.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string Instructor { get; set; }
        public int MaxAttendees { get; set; }
        // Helper method to check if the course is full
        public bool IsFull()
        {
            return Enrollments.Count >= MaxAttendees;
        }
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>(); // Navigation property for enrollments



    }
}
