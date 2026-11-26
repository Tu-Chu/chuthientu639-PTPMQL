using System.ComponentModel.DataAnnotations;

namespace DemoMVC.Models
{
    public class RegisterViewModel
    {
        [Required, EmailAddress, Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), MinLength(8), Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), Compare(nameof(Password)), Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginViewModel
    {
        [Required, EmailAddress, Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress, Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordViewModel
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), MinLength(8), Display(Name = "Mật khẩu mới")]
        public string Password { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), Compare(nameof(Password)), Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ChangePasswordViewModel
    {
        [Required, DataType(DataType.Password), Display(Name = "Mật khẩu hiện tại")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), MinLength(8), Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), Compare(nameof(NewPassword)), Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
