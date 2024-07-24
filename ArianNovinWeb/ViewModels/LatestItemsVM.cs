using ArianNovinWeb.Models;

namespace ArianNovinWeb.ViewModels
{
    public class LatestItemsVM
    {
        public string Title {  get; set; }
        public List<Post> Items { get; set; }
    }
}
