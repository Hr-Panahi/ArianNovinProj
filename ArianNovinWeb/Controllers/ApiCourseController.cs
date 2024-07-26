using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArianNovinWeb.Data;
using ArianNovinWeb.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class CourseApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CourseApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Course>>> GetCourses()
    {
        var courses = await _context.Courses
            .Include(c => c.Enrollments)
            .ThenInclude(e => e.User)
            .ToListAsync();

        var result = courses.Select(course => new
        {
            course.CourseId,
            course.Title,
            course.Description,
            course.StartDate,
            course.EndDate,
            course.Instructor,
            course.MaxAttendees,
            Enrollments = course.Enrollments.Select(e => new
            {
                e.EnrollmentId,
                e.EnrolledAt,
                User = new { e.User.UserName, e.User.Email }
            })
        });

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Course>> GetCourse(int id)
    {
        var course = await _context.Courses
            .Include(c => c.Enrollments)
            .ThenInclude(e => e.User)
            .FirstOrDefaultAsync(c => c.CourseId == id);

        if (course == null)
        {
            return NotFound();
        }

        var result = new
        {
            course.CourseId,
            course.Title,
            course.Description,
            course.StartDate,
            course.EndDate,
            course.Instructor,
            course.MaxAttendees,
            Enrollments = course.Enrollments.Select(e => new
            {
                e.EnrollmentId,
                e.EnrolledAt,
                User = new { e.User.UserName, e.User.Email }
            })
        };

        return Ok(result);
    }
}
