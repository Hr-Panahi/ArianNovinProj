using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArianNovinWeb.Data;
using ArianNovinWeb.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ArianNovinWeb.Controllers
{
    public class PostController : Controller
    {
        //Dependency injection
        private readonly ApplicationDbContext _context;

        public PostController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            IEnumerable<Post> posts = _context.Posts;
            return View(posts);
        }

        #region Create
        //Get
        public IActionResult Create()
        {
            return View();
        }
        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Post post)
        {
            if(ModelState.IsValid)
            {
                _context.Posts.Add(post);
                _context.SaveChanges();
                TempData["successCreate"] = "Post Created Successfully";
                return RedirectToAction("Index");
            }
            return View(post);
        }
        #endregion
    }

}
