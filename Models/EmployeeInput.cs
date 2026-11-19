using System.ComponentModel.DataAnnotations;

namespace MvcAjaxModalCrud.Models
{
    public class EmployeeInput
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(120, ErrorMessage = "Họ tên tối đa 120 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(254, ErrorMessage = "Email tối đa 254 ký tự")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phòng ban không được để trống")]
        [StringLength(80, ErrorMessage = "Phòng ban tối đa 80 ký tự")]
        public string Department { get; set; } = string.Empty;

        [Range(typeof(decimal), "0", "999999999999", ErrorMessage = "Lương phải từ 0 đến 999999999999")]
        public decimal Salary { get; set; }
    }
}