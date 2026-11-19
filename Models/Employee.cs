using System.ComponentModel.DataAnnotations;

namespace MvcAjaxModalCrud.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(120)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phòng ban không được để trống")]
        [StringLength(80)]
        public string Department { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Lương phải lớn hơn hoặc bằng 0")]
        public decimal Salary { get; set; }
    }
}