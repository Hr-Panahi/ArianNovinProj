using ArianNovinWeb.Models;

namespace ArianNovinWeb.ViewModels
{
    public class PostIndexVM
    {
        public IEnumerable<Post> Posts { get; set; }
        public PostNavigationVM PostNavigation { get; set; }
        public LatestItemsVM LatestPosts { get; set; }
        public bool ShowShareButton { get; set; }
        public IEnumerable<Comment> Comments { get; set; }
    }

}
