namespace DemoMVC.Models
{
    public class DynamicPermissionRoleViewModel
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public List<DynamicPermissionActionViewModel> Actions { get; set; } = new();
    }

    public class DynamicPermissionActionViewModel
    {
        public string ControllerName { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = "ANY";
        public string DisplayName => $"{ControllerName}.{ActionName}";
        public bool IsAssigned { get; set; }
    }

    public class DynamicPermissionUpdateViewModel
    {
        public string RoleId { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
    }
}
