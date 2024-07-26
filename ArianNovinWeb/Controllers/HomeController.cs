using ArianNovinWeb.Data;
using ArianNovinWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ArianNovinWeb.Controllers
{
	public class HomeController : Controller
	{
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

		public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
		{
            _context = context;
            _logger = logger;
		}

        public async Task<IActionResult> Index()
        {
            var latestPosts = await _context.Posts
                .Include(p => p.Author)
                .OrderByDescending(p => p.CreateDate)
                .Take(5)
                .ToListAsync();

            ViewBag.LatestPosts = latestPosts;
            return View();
        }

        public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
