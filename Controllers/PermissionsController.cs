using System.Reflection;
using DemoMVC.Data;
using DemoMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;

namespace DemoMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PermissionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public PermissionsController(ApplicationDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.OrderBy(role => role.Name).ToListAsync();
            var roleItems = roles.Select(role => new
            {
                role.Id,
                Name = role.Name ?? string.Empty
            }).ToList();

            ViewBag.Roles = roleItems;

            var actions = GetActionKeys();
            var roleId = roles.FirstOrDefault()?.Id;
            if (roleId == null) return View(new DynamicPermissionRoleViewModel());

            return await Matrix(roleId);
        }

        [HttpGet]
        public async Task<IActionResult> Matrix(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return NotFound();

            var allActions = GetActionKeys();
            var assigned = await _context.RolePermissions
                .Where(permission => permission.RoleId == roleId)
                .ToListAsync();

            var model = new DynamicPermissionRoleViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name ?? string.Empty,
                Actions = allActions.Select(action => new DynamicPermissionActionViewModel
                {
                    ControllerName = action.Controller,
                    ActionName = action.Action,
                    HttpMethod = action.HttpMethod,
                    IsAssigned = assigned.Any(permission =>
                        permission.ControllerName.Equals(action.Controller, StringComparison.OrdinalIgnoreCase) &&
                        permission.ActionName.Equals(action.Action, StringComparison.OrdinalIgnoreCase))
                }).ToList()
            };

            ViewBag.Roles = await _roleManager.Roles
                .OrderBy(item => item.Name)
                .Select(item => new { item.Id, Name = item.Name ?? string.Empty })
                .ToListAsync();

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(DynamicPermissionUpdateViewModel model)
        {
            var role = await _roleManager.FindByIdAsync(model.RoleId);
            if (role == null) return NotFound();

            var existing = await _context.RolePermissions
                .Where(permission => permission.RoleId == model.RoleId)
                .ToListAsync();
            _context.RolePermissions.RemoveRange(existing);

            foreach (var key in model.Permissions ?? new List<string>())
            {
                var parts = key.Split('|');
                if (parts.Length != 3) continue;

                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = model.RoleId,
                    ControllerName = parts[0],
                    ActionName = parts[1],
                    HttpMethod = parts[2]
                });
            }

            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = $"Đã cập nhật phân quyền cho role {role.Name}.";
            return RedirectToAction(nameof(Matrix), new { roleId = model.RoleId });
        }

        private static List<(string Controller, string Action, string HttpMethod)> GetActionKeys()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var result = new List<(string Controller, string Action, string HttpMethod)>();

            foreach (var controllerType in assembly.GetTypes()
                         .Where(type => typeof(Controller).IsAssignableFrom(type) &&
                                        type.Name.EndsWith("Controller", StringComparison.Ordinal)))
            {
                var controllerName = controllerType.Name.Replace("Controller", "", StringComparison.Ordinal);

                foreach (var method in controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                             .Where(method => method.DeclaringType == controllerType))
                {
                    if (method.GetCustomAttribute<AllowAnonymousAttribute>() != null) continue;

                    var httpAttributes = method.GetCustomAttributes<HttpMethodAttribute>();
                    var httpMethod = httpAttributes.SelectMany(attribute => attribute.HttpMethods)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .FirstOrDefault() ?? "ANY";

                    result.Add((controllerName, method.Name, httpMethod));
                }
            }

            return result
                .Distinct()
                .OrderBy(item => item.Controller)
                .ThenBy(item => item.Action)
                .ToList();
        }
    }
}
