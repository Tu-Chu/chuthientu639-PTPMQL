using System.ComponentModel.DataAnnotations;

namespace DemoMVC.Models
{
    public class RoleListItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int UserCount { get; set; }
    }

    public class RoleEditViewModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Tên role không được để trống")]
        [StringLength(100, ErrorMessage = "Tên role tối đa 100 ký tự")]
        [Display(Name = "Tên role")]
        public string Name { get; set; } = string.Empty;
    }

    public class UserRoleViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<UserRoleItemViewModel> Roles { get; set; } = new();
    }

    public class UserRoleItemViewModel
    {
        public string RoleName { get; set; } = string.Empty;
        public bool IsAssigned { get; set; }
    }
}
