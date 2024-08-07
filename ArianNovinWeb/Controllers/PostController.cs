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
using Microsoft.AspNetCore.Antiforgery;

namespace ArianNovinWeb.Controllers
{
    [Authorize]
    public class PostController : Controller
    {


        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        /// <summary>
        /// Constructor for the PostController class.
        /// </summary>
        /// <param name="context">ApplicationDbContext instance</param>
        /// <param name="userManager">UserManager instance</param>
        /// <param name="webHostEnvironment">WebHostEnvironment instance</param>
        public PostController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;

        }

        #region Index
        /// <summary>
        /// Displays the list of posts with the latest post displayed.
        /// </summary>
        /// <param name="id">Optional post ID</param>
        /// <returns>Index view with posts</returns>
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
                var emptyViewModel = new PostIndexVM
                {
                    Posts = posts,
                    ShowShareButton = true
                };
                return View(emptyViewModel);
            }

            if (id == null)
            {
                //id = posts.First().PostId;
                id = posts.LastOrDefault().PostId;
            }

            var currentPost = posts.FirstOrDefault(p => p.PostId == id);

            if (currentPost == null)
            {
                return NotFound("Post not found.");
            }

            var currentIndex = posts.IndexOf(currentPost);
            var previousPostId = currentIndex > 0 ? posts[currentIndex - 1].PostId : (int?)null;
            var nextPostId = currentIndex < posts.Count - 1 ? posts[currentIndex + 1].PostId : (int?)null;

            var postNavigation = new PostNavigationVM
            {
                Post = currentPost,
                PreviousPostId = previousPostId,
                NextPostId = nextPostId
            };

            var populatedViewModel = new PostIndexVM
            {
                Posts = posts,
                PostNavigation = postNavigation,
                ShowShareButton = false,
                LatestPosts = latestPostsViewModel
            };

            return View(populatedViewModel);
        }
        #endregion

        #region Create
        /// <summary>
        /// Displays the Create Post form.
        /// </summary>
        /// <returns>Create view</returns>
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

        /// <summary>
        /// Handles the submission of the Create Post form.
        /// </summary>
        /// <param name="post">Post model</param>
        /// <param name="imageFile">Uploaded image file</param>
        /// <returns>Redirects to Index view</returns>
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
            TempData["SuccessMessage"] = "Post Created Successfully";
            return RedirectToAction(nameof(Index));

        }
        #endregion

        #region Edit
        /// <summary>
        /// Displays the Edit Post form.
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <returns>Edit view</returns>
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

        /// <summary>
        /// Handles the submission of the Edit Post form.
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <param name="post">Post model</param>
        /// <param name="imageFile">Uploaded image file</param>
        /// <returns>Redirects to Index view</returns>
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
                TempData["SuccessMessage"] = "Post Edited Successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }
        #endregion

        #region Delete
        /// <summary>
        /// Displays the Delete Post confirmation view.
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <returns>Delete view</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);

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

        /// <summary>
        /// Handles the deletion of a post and its related comments.
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <returns>Redirects to Index view</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Comments) // Include related comments
                .FirstOrDefaultAsync(p => p.PostId == id); var userId = _userManager.GetUserId(User);

            if (post.AuthorId != userId)
            {
                return Forbid();
            }

            // Delete related comments first
            if (post.Comments != null)
            {
                DeleteComments(post.Comments);
            }

            // Then delete the post
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Post deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Recursively deletes comments and their replies.
        /// </summary>
        /// <param name="comments">Enumerable of comments</param>
        private void DeleteComments(IEnumerable<Comment> comments)
        {
            foreach (var comment in comments)
            {
                if (comment.Replies.Any())
                {
                    DeleteComments(comment.Replies);
                }
                _context.Comments.Remove(comment);
            }
        }
        #endregion

        /// <summary>
        /// Checks if a post exists by ID.
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <returns>Boolean indicating if post exists</returns>
        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.PostId == id);
        }

        #region Details
        /// <summary>
        /// Displays the details of a post.
        /// </summary>
        /// <param name="id">Post ID</param>
        /// <returns>Details view</returns>
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
        #endregion

        #region Add Comment
        /// <summary>
        /// Handles adding a comment to a post.
        /// </summary>
        /// <param name="postId">Post ID</param>
        /// <param name="content">Comment content</param>
        /// <returns>Redirects to Index view</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int postId, string content)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Failed to add comment. Please try again.";
                return RedirectToAction("Index", new { id = postId });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Forbid();
            }

            var comment = new Comment
            {
                PostId = postId,
                Content = content,
                AuthorId = user.Id,
                CreatedAt = DateTime.Now,
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Comment added successfully!";
            return RedirectToAction("Index", new { id = postId });
        }
        #endregion
        #region Add Reply
        /// <summary>
        /// Handles adding a reply to a comment.
        /// </summary>
        /// <param name="postId">Post ID</param>
        /// <param name="content">Comment content</param>
        /// <param name="parentCommentId">Parent comment ID (optional)</param>
        /// <returns>Redirects to Details view</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReply(int postId, string content, int parentCommentId)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Failed to add reply. Please try again.";
                return RedirectToAction("Index", new { id = postId });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Forbid();
            }

            var reply = new Comment
            {
                PostId = postId,
                Content = content,
                AuthorId = user.Id,
                CreatedAt = DateTime.Now,
                ParentCommentId = parentCommentId
            };

            _context.Comments.Add(reply);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Reply added successfully!";
            return RedirectToAction("Details", new { id = postId });
        }
        #endregion

    }

}