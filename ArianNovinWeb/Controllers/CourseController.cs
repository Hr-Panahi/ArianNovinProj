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

    /// <summary>
    /// Constructor for the CourseController class.
    /// </summary>
    /// <param name="context">ApplicationDbContext instance</param>
    public CourseController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Displays the list of courses.
    /// </summary>
    /// <returns>Index view with courses</returns>
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var courses = await _context.Courses
            .Include(c => c.Enrollments)
            .ThenInclude(e => e.User)
            .ToListAsync();
        return View(courses);
    }

    #region Create Course
    /// <summary>
    /// Displays the Create Course form.
    /// </summary>
    /// <returns>Create view</returns>
    public IActionResult Create()
    {
        return View(new CourseVM());
    }

    /// <summary>
    /// Handles the submission of the Create Course form.
    /// </summary>
    /// <param name="model">CourseViewModel instance</param>
    /// <returns>Redirects to Index view</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseVM model)
    {
        if (ModelState.IsValid)
        {
            var course = new Course
            {
                Title = model.Title,
                Description = model.Description,
                StartDate = model.StartDate,
                Instructor = model.Instructor,
                EndDate = model.EndDate,
                MaxAttendees = model.MaxAttendees
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index)); // Redirect to a list of courses or another relevant page
        }
        return View(model);
    }
    #endregion

    #region Edit Course
    /// <summary>
    /// Displays the Edit Course form.
    /// </summary>
    /// <param name="id">Course ID</param>
    /// <returns>Edit view</returns>
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

    /// <summary>
    /// Handles the submission of the Edit Course form.
    /// </summary>
    /// <param name="id">Course ID</param>
    /// <param name="course">Course model</param>
    /// <returns>Redirects to Index view</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("CourseId,Title,Description,StartDate,EndDate,Instructor,MaxAttendees")] Course course)
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
    #endregion

    #region Delete Course
    /// <summary>
    /// Displays the Delete Course confirmation view.
    /// </summary>
    /// <param name="id">Course ID</param>
    /// <returns>Delete view</returns>
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

    /// <summary>
    /// Handles the deletion of a course.
    /// </summary>
    /// <param name="id">Course ID</param>
    /// <returns>Redirects to Index view</returns>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    #endregion

    /// <summary>
    /// Checks if a course exists.
    /// </summary>
    /// <param name="id">Course ID</param>
    /// <returns>Boolean indicating if the course exists</returns>
    private bool CourseExists(int id)
    {
        return _context.Courses.Any(e => e.CourseId == id);
    }

    #region Enroll in Course
    /// <summary>
    /// Handles user enrollment in a course.
    /// </summary>
    /// <param name="courseId">Course ID</param>
    /// <param name="returnUrl">Return URL</param>
    /// <returns>Redirects to the course details or login page if not authenticated</returns>
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

        var course = await _context.Courses
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.CourseId == courseId);

        if (course == null)
        {
            return NotFound();
        }

        if(course.Enrollments.Count >= course.MaxAttendees)
        {
            TempData["ErrorMessage"] = "This course is already full.";
            return RedirectToAction("Details", new { id = courseId });
        }

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
    #endregion

    #region Details of the Course
    /// <summary>
    /// Displays the details of a course.
    /// </summary>
    /// <param name="id">Course ID</param>
    /// <returns>Details view</returns>
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
    #endregion
}
