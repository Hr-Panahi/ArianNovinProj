using ArianNovinWeb.Models;
using ArianNovinWeb.ViewModels;

namespace ArianNovinWeb.ViewModels
{
    public class PostIndexViewModel
    {
        public List<Post> Posts { get; set; }
        public PostNavigationViewModel PostNavigation { get; set; }
        public LatestItemsVM LatestPosts { get; set; }
        public bool ShowShareButton { get; set; }
    }

}
