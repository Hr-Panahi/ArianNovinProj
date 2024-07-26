using ArianNovinWeb.Models;
using ArianNovinWeb.ViewModels;

namespace ArianNovinWeb.ViewModels
{
    public class PostIndexVM
    {
        public List<Post> Posts { get; set; }
        public PostNavigationVM PostNavigation { get; set; }
        public LatestItemsVM LatestPosts { get; set; }
        public bool ShowShareButton { get; set; }
        public List<Comment> Comments { get; set; }
    }

}
