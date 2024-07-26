using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ArianNovinWeb.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ArianNovinWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiUserController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ApiUserController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: api/ApiUser
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserVM>>> GetUsers()
        {
            var users = await _userManager.Users.Select(u => new UserVM
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                Roles = _userManager.GetRolesAsync(u).Result
            }).ToListAsync();

            return users;
        }

        // GET: api/ApiUser/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<EditUserVM>> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditUserVM
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            return model;
        }

        // PUT: api/ApiUser/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutUser(string id, EditUserVM model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }

            return NoContent();
        }

        // GET: api/ApiUser/EditRoles/{userId}
        [HttpGet("EditRoles/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<EditUserRoleVM>> EditRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.Select(r => r.Name).Distinct().ToListAsync();

            var model = new EditUserRoleVM
            {
                UserId = user.Id,
                AvailableRoles = allRoles ?? new List<string>(),
                SelectedRoles = userRoles
            };

            return model;
        }

        // PUT: api/ApiUser/EditRoles/{userId}
        [HttpPut("EditRoles/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditRoles(string userId, EditUserRoleVM model)
        {
            if (userId != model.UserId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var rolesToAdd = model.SelectedRoles.Except(userRoles).ToList();
            var rolesToRemove = userRoles.Except(model.SelectedRoles).ToList();

            var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded)
            {
                foreach (var error in addResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }

            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                foreach (var error in removeResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }

            return NoContent();
        }
    }
}
