using Microsoft.AspNetCore.Mvc;

namespace ArianNovinWeb.Controllers
{
	public class LearningController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Forum(string name, int numTimes = 1)
		{
            ViewData["Message"] = "Hello " + name;
            ViewData["NumTimes"] = numTimes;
            return View();
        }
	}
}
