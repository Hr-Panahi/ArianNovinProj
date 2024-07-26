using Microsoft.AspNetCore.Mvc;
using ArianNovinWeb.Models;
using Microsoft.EntityFrameworkCore;
using ArianNovinWeb.Data;
using ArianNovinWeb.ViewModels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ArianNovinWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiCourseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApiCourseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ApiCourse
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Course>>> GetCourses()
        {
            return await _context.Courses
                .Include(c => c.Enrollments)
                .ThenInclude(e => e.User)
                .ToListAsync();
        }

        // GET: api/ApiCourse/5
        [HttpGet("{id}")]
        [AllowAnonymous]
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

            return course;
        }

        // POST: api/ApiCourse
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Course>> PostCourse(CourseVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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

            return CreatedAtAction(nameof(GetCourse), new { id = course.CourseId }, course);
        }

        // PUT: api/ApiCourse/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutCourse(int id, Course course)
        {
            if (id != course.CourseId)
            {
                return BadRequest();
            }

            _context.Entry(course).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/ApiCourse/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.CourseId == id);
        }
    }
}
