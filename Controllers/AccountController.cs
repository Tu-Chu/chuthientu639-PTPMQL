using DemoMVC.Models;
using DemoMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DemoMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationUrl = Url.Action(
                    nameof(ConfirmEmail),
                    "Account",
                    new { userId = user.Id, token },
                    Request.Scheme)!;

                await _emailSender.SendAsync(
                    user.Email!,
                    "Xác thực tài khoản DemoMVC",
                    $"<h2>Chào mừng bạn đến với DemoMVC</h2><p>Vui lòng xác thực tài khoản:</p><p><a href='{confirmationUrl}'>Xác thực tài khoản</a></p>");

                TempData["StatusMessage"] = "Đăng ký thành công. Vui lòng kiểm tra email để xác thực tài khoản.";
                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            var model = new LoginViewModel { ReturnUrl = returnUrl };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
                return LocalRedirect(string.IsNullOrEmpty(model.ReturnUrl) ? "/" : model.ReturnUrl);

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản tạm thời bị khóa do đăng nhập sai nhiều lần.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied() => View();

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest("Liên kết xác thực không hợp lệ.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("Không tìm thấy tài khoản.");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            TempData["StatusMessage"] = result.Succeeded
                ? "Xác thực email thành công. Bạn có thể đăng nhập."
                : "Liên kết xác thực không hợp lệ hoặc đã hết hạn.";
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetUrl = Url.Action(nameof(ResetPassword), "Account",
                    new { userId = user.Id, token }, Request.Scheme)!;

                await _emailSender.SendAsync(
                    user.Email!,
                    "Đặt lại mật khẩu DemoMVC",
                    $"<h2>Yêu cầu đặt lại mật khẩu</h2><p><a href='{resetUrl}'>Tạo mật khẩu mới</a></p><p>Liên kết có hiệu lực theo cấu hình bảo mật của hệ thống.</p>");
            }

            TempData["StatusMessage"] = "Nếu email tồn tại trong hệ thống, liên kết đặt lại mật khẩu đã được gửi.";
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation() => View();

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest("Liên kết đặt lại mật khẩu không hợp lệ.");

            return View(new ResetPasswordViewModel { UserId = userId, Token = token });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return RedirectToAction(nameof(ResetPasswordConfirmation));

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded) return RedirectToAction(nameof(ResetPasswordConfirmation));

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation() => View();

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword() => View();

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Login));

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["StatusMessage"] = "Đổi mật khẩu thành công.";
                return RedirectToAction(nameof(ChangePassword));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }
    }
}
