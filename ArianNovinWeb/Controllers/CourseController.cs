using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ArianNovinWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ArianNovinWeb.Data;
using ArianNovinWeb.ViewModels;
using System.Security.Claims;

[Authorize(Roles = "Admin")]
public class CourseController : Controller
{
    private readonly ApplicationDbContext _context;

    public CourseController(ApplicationDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var courses = await _context.Courses
            .Include(c => c.Enrollments)
            .ThenInclude(e => e.User)
            .ToListAsync();
        return View(courses);
    }

    public IActionResult Create()
    {
        return View(new CourseViewModel());
    }

    // POST: /Course/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseViewModel model)
    {
        if (ModelState.IsValid)
        {
            var course = new Course
            {
                Title = model.Title,
                Description = model.Description,
                StartDate = model.StartDate,
                Instructor = model.Instructor,
                EndDate = model.EndDate
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index)); // Redirect to a list of courses or another relevant page
        }
        return View(model);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound();
        }
        return View(course);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("CourseId,Title,Description,StartDate,EndDate,Instructor")] Course course)
    {
        if (id != course.CourseId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(course);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(course.CourseId))
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
        return View(course);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var course = await _context.Courses
            .FirstOrDefaultAsync(m => m.CourseId == id);
        if (course == null)
        {
            return NotFound();
        }

        return View(course);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool CourseExists(int id)
    {
        return _context.Courses.Any(e => e.CourseId == id);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int courseId, string returnUrl = null)
    {
        if (!User.Identity.IsAuthenticated)
        {
            returnUrl = Url.Action("Index", "Course",new {id=courseId});
            return RedirectToPage("/Account/Login", new { area = "Identity", ReturnUrl = returnUrl });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get logged-in user ID

        // Check if the user is already enrolled in the course
        var existingEnrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.CourseId == courseId && e.UserId == userId);

        if (existingEnrollment == null)
        {
            var enrollment = new Enrollment
            {
                CourseId = courseId,
                UserId = userId,
                EnrolledAt = DateTime.Now
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();
        }

        // If there's a returnUrl and it points to Enroll, redirect to Details instead
        if (!string.IsNullOrEmpty(returnUrl) && returnUrl.Contains("Enroll"))
        {
            return RedirectToAction("Details", new { id = courseId });
        }

        // Redirect to the details page of the course
        return RedirectToAction("Details", new { id = courseId });
    }



    [AllowAnonymous]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var course = await _context.Courses
            .Include(c => c.Enrollments)
            .ThenInclude(e => e.User)
            .FirstOrDefaultAsync(m => m.CourseId == id);

        if (course == null)
        {
            return NotFound();
        }

        return View(course);
    }
}
