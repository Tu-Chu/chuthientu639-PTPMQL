using DemoMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemoMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles
                .OrderBy(role => role.Name)
                .ToListAsync();

            var model = new List<RoleListItemViewModel>();
            foreach (var role in roles)
            {
                model.Add(new RoleListItemViewModel
                {
                    Id = role.Id,
                    Name = role.Name ?? string.Empty,
                    UserCount = (await _userManager.GetUsersInRoleAsync(role.Name!)).Count
                });
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Create() => View(new RoleEditViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleEditViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var name = model.Name.Trim();
            if (await _roleManager.RoleExistsAsync(name))
            {
                ModelState.AddModelError(nameof(model.Name), "Tên role đã tồn tại.");
                return View(model);
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(name));
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            TempData["StatusMessage"] = "Tạo role thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            return View(new RoleEditViewModel { Id = role.Id, Name = role.Name ?? string.Empty });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoleEditViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var role = await _roleManager.FindByIdAsync(model.Id ?? string.Empty);
            if (role == null) return NotFound();

            var name = model.Name.Trim();
            var existing = await _roleManager.FindByNameAsync(name);
            if (existing != null && existing.Id != role.Id)
            {
                ModelState.AddModelError(nameof(model.Name), "Tên role đã được sử dụng.");
                return View(model);
            }

            role.Name = name;
            role.NormalizedName = name.ToUpperInvariant();
            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            TempData["StatusMessage"] = "Cập nhật role thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            if (role.Name == "Admin")
            {
                TempData["StatusError"] = "Không thể xóa role Admin mặc định.";
                return RedirectToAction(nameof(Index));
            }

            var users = await _userManager.GetUsersInRoleAsync(role.Name!);
            if (users.Count > 0)
            {
                TempData["StatusError"] = "Không thể xóa role đang được gán cho người dùng.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                TempData["StatusError"] = string.Join(" ", result.Errors.Select(error => error.Description));
            }
            else
            {
                TempData["StatusMessage"] = "Xóa role thành công.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> UserRoles()
        {
            var users = await _userManager.Users.OrderBy(user => user.Email).ToListAsync();
            var roles = await _roleManager.Roles.OrderBy(role => role.Name).ToListAsync();

            var model = new List<UserRoleViewModel>();
            foreach (var user in users)
            {
                var assignedRoles = await _userManager.GetRolesAsync(user);
                model.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    Email = user.Email ?? user.UserName ?? string.Empty,
                    Roles = roles.Select(role => new UserRoleItemViewModel
                    {
                        RoleName = role.Name ?? string.Empty,
                        IsAssigned = assignedRoles.Contains(role.Name ?? string.Empty)
                    }).ToList()
                });
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRoles(string userId, List<string> roleNames)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            var validRoleNames = await _roleManager.Roles
                .Select(role => role.Name!)
                .ToListAsync();
            roleNames ??= new List<string>();

            var selectedRoles = roleNames.Where(validRoleNames.Contains).Distinct().ToList();
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles.Except(selectedRoles));
            if (!removeResult.Succeeded)
            {
                TempData["StatusError"] = string.Join(" ", removeResult.Errors.Select(error => error.Description));
                return RedirectToAction(nameof(UserRoles));
            }

            var addResult = await _userManager.AddToRolesAsync(user, selectedRoles.Except(currentRoles));
            if (!addResult.Succeeded)
            {
                TempData["StatusError"] = string.Join(" ", addResult.Errors.Select(error => error.Description));
            }
            else
            {
                TempData["StatusMessage"] = "Cập nhật role người dùng thành công.";
            }

            return RedirectToAction(nameof(UserRoles));
        }
    }
}
