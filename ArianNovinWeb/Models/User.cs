using System.ComponentModel.DataAnnotations;

namespace ArianNovinWeb.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        [Required]
        [StringLength(50)]
        public string FirstName { get;set; }
        [Required]
        [StringLength(50)]
        public string LastName { get;set; }
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }
        [Phone]
        public string PhoneNumber { get; set; }

    }
}
