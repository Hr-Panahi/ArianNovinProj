using ArianNovinWeb.Models;

namespace ArianNovinWeb.ViewModels
{
    public class PostNavigationViewModel
    {
        public Post Post { get; set; }
        public int? PreviousPostId { get; set; }
        public int? NextPostId { get; set; }
    }
}
