using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ArianNovinWeb.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class UserController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    /// <summary>
    /// Constructor for the UserController class.
    /// </summary>
    /// <param name="userManager">UserManager instance</param>
    /// <param name="roleManager">RoleManager instance</param>
    public UserController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    #region Edit
    /// <summary>
    /// Displays the user's profile for editing.
    /// </summary>
    /// <returns>Edit view with user details</returns>
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
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

        return View(model);
    }

    /// <summary>
    /// Updates the user's profile.
    /// </summary>
    /// <param name="model">EditUserVM instance</param>
    /// <returns>Redirects to Edit view or returns to Edit view if there is an error</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserVM model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(model.Id);
        if (user == null)
        {
            return NotFound();
        }

        user.UserName = model.UserName;
        user.Email = model.Email;
        user.PhoneNumber = model.PhoneNumber;
        

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Your profile has been updated";
            return RedirectToAction(nameof(Edit));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }
    #endregion

    #region Check Users
    /// <summary>
    /// Displays the list of users for admin.
    /// </summary>
    /// <returns>Index view with users and their roles</returns>
    [Authorize(Roles = "Admin")]
    public IActionResult Index()
    {
        var users = _userManager.Users.Select(u => new UserVM
        {
            Id = u.Id,
            UserName = u.UserName,
            Email = u.Email,
            Roles = _userManager.GetRolesAsync(u).Result
        }).ToList();

        return View(users);
    }
    #endregion

    #region Edit User Roles
    /// <summary>
    /// Displays the edit roles form for a specific user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>EditRoles view with user roles</returns>
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EditRoles(string userId)
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

        return View(model);
    }

    /// <summary>
    /// Updates the roles of a user.
    /// </summary>
    /// <param name="model">EditUserRoleVM instance</param>
    /// <returns>Redirects to Index view or returns to EditRoles view if there is an error</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EditRoles(EditUserRoleVM model)
    {
        if (model.AvailableRoles == null || !model.AvailableRoles.Any())
        {
            model.AvailableRoles = await _roleManager.Roles.Select(r => r.Name).Distinct().ToListAsync();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
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
            return View(model);
        }

        var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
        if (!removeResult.Succeeded)
        {
            foreach (var error in removeResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }
    #endregion
}
