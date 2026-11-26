using DemoMVC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DemoMVC.Authorization
{
    public class DynamicPermissionFilter : IAsyncAuthorizationFilter
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DynamicPermissionFilter(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var endpoint = context.HttpContext.GetEndpoint();
            var allowAnonymous = endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null;
            if (allowAnonymous) return;

            var user = context.HttpContext.User;
            if (user.Identity?.IsAuthenticated != true)
            {
                context.Result = new ChallengeResult();
                return;
            }

            if (user.IsInRole("Admin")) return;

            var controller = context.RouteData.Values["controller"]?.ToString() ?? string.Empty;
            var action = context.RouteData.Values["action"]?.ToString() ?? string.Empty;
            var method = context.HttpContext.Request.Method;

            var identityUser = await _userManager.GetUserAsync(user);
            if (identityUser == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            var roleNames = await _userManager.GetRolesAsync(identityUser);
            if (roleNames.Count == 0)
            {
                context.Result = new ForbidResult();
                return;
            }

            var roleManager = context.HttpContext.RequestServices.GetRequiredService<RoleManager<IdentityRole>>();
            var roleIds = new List<string>();
            foreach (var roleName in roleNames)
            {
                var role = await roleManager.FindByNameAsync(roleName);
                if (role != null) roleIds.Add(role.Id);
            }

            var hasPermission = _context.RolePermissions.Any(permission =>
                roleIds.Contains(permission.RoleId) &&
                permission.ControllerName.Equals(controller, StringComparison.OrdinalIgnoreCase) &&
                permission.ActionName.Equals(action, StringComparison.OrdinalIgnoreCase) &&
                (permission.HttpMethod == "ANY" || permission.HttpMethod.Equals(method, StringComparison.OrdinalIgnoreCase)));

            if (!hasPermission)
                context.Result = new ForbidResult();
        }
    }
}
