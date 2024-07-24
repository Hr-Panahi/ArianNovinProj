using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArianNovinWeb.Data;
using ArianNovinWeb.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using ArianNovinWeb.ViewModels;

namespace ArianNovinWeb.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PostController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;

        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(int? id)
        {
            var posts = await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Comments)
                .ThenInclude(c => c.Author)
                .ToListAsync();

            //Latest Posts Section
            var latestPosts = await _context.Posts
                .OrderByDescending(p => p.CreateDate)
                .Take(5)
                .ToListAsync();

            var latestPostsViewModel = new LatestItemsVM
            {
                Title = "Latest Posts",
                Items = latestPosts
            };
            //Latest Post

            if (!posts.Any())
            {
                var emptyViewModel = new PostIndexViewModel
                {
                    Posts = posts,
                    ShowShareButton = true
                };
                return View(emptyViewModel);
            }

            if (id == null)
            {
                id = posts.First().PostId;
            }

            var currentPost = posts.FirstOrDefault(p => p.PostId == id);

            if (currentPost == null)
            {
                return NotFound("Post not found.");
            }

            var currentIndex = posts.IndexOf(currentPost);
            var previousPostId = currentIndex > 0 ? posts[currentIndex - 1].PostId : (int?)null;
            var nextPostId = currentIndex < posts.Count - 1 ? posts[currentIndex + 1].PostId : (int?)null;

            var postNavigation = new PostNavigationViewModel
            {
                Post = currentPost,
                PreviousPostId = previousPostId,
                NextPostId = nextPostId
            };

            var populatedViewModel = new PostIndexViewModel
            {
                Posts = posts,
                PostNavigation = postNavigation,
                ShowShareButton = false,
                LatestPosts = latestPostsViewModel
            };

            return View(populatedViewModel);
        }

        #region Create
        [Authorize]
        public IActionResult Create()
        {
            var post = new Post
            {
                AuthorId = _userManager.GetUserId(User),
                Author = _context.Users.FirstOrDefault(u => u.Id == _userManager.GetUserId(User)),
                CreateDate = DateTime.Now
            };
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(Post post, IFormFile imageFile)
        {

            #region Uploading file
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (imageFile != null)
            {
                string fileName = Guid.NewGuid().ToString();
                var uploads = Path.Combine(wwwRootPath, @"images\");
                var extension = Path.GetExtension(imageFile.FileName);

                if (post.ImagePath != null) //deleting image if already exists
                {
                    var oldImagePath = Path.Combine(wwwRootPath, post.ImagePath.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                {
                    imageFile.CopyTo(fileStreams);
                }
                post.ImagePath = @"\images\" + fileName + extension;
            }
            #endregion

            post.CreateDate = DateTime.Now;
            post.AuthorId = _userManager.GetUserId(User);
            _context.Add(post);
            await _context.SaveChangesAsync();
            TempData["successCreate"] = "Post Created Successfully";
            return RedirectToAction(nameof(Index));

        }
        #endregion

        #region Edit
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (post.AuthorId != userId)
            {
                return Forbid();
            }

            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, Post post, IFormFile imageFile)
        {
            var existingPost = await _context.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.PostId == id);

            if (id != post.PostId)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (post.AuthorId != userId)
            {
                return Forbid(); // Access denied
            }
            ModelState.Remove("imageFile");
            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var fileName = Path.GetFileName(imageFile.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        post.ImagePath = "/images/" + fileName;
                    }

                    _context.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.PostId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }
        #endregion

        #region Delete
        [Authorize]
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var post = await _context.Posts
        //        .FirstOrDefaultAsync(m => m.PostId == id);
        //    if (post == null)
        //    {
        //        return NotFound();
        //    }

        //    var userId = _userManager.GetUserId(User);
        //    if (post.AuthorId != userId)
        //    {
        //        return Forbid();
        //    }

        //    return View(post);
        //}
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return Json(new { success = false, message = "Post not found." });
            }

            var userId = _userManager.GetUserId(User);
            if (post.AuthorId != userId)
            {
                return Forbid();
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //[Authorize]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var post = await _context.Posts.FindAsync(id);
        //    var userId = _userManager.GetUserId(User);
        //    if (post.AuthorId != userId)
        //    {
        //        return Forbid();
        //    }

        //    _context.Posts.Remove(post);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}
        #endregion

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.PostId == id);
        }


        // GET: Post/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(Comment comment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                comment.AuthorId = user.Id;
                comment.CreatedAt = DateTime.Now;
                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = comment.PostId });
        }

    }

}