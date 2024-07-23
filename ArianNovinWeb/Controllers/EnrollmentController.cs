using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ArianNovinWeb.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using ArianNovinWeb.Data;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class EnrollmentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public EnrollmentController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var enrollments = await _context.Enrollments
            .Include(e => e.Course)
            .Where(e => e.UserId == userId)
            .ToListAsync();

        return View(enrollments);
    }

    public async Task<IActionResult> Enroll(int courseId)
    {
        var userId = _userManager.GetUserId(User);
        if (!_context.Enrollments.Any(e => e.CourseId == courseId && e.UserId == userId))
        {
            var enrollment = new Enrollment
            {
                CourseId = courseId,
                UserId = userId
            };
            _context.Add(enrollment);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index", "Course");
    }
}
