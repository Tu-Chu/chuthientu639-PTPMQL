using System.ComponentModel.DataAnnotations;

namespace DemoMVC.Models
{
    public class RolePermission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string RoleId { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string ControllerName { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string ActionName { get; set; } = string.Empty;

        [StringLength(20)]
        public string HttpMethod { get; set; } = "ANY";
    }
}
