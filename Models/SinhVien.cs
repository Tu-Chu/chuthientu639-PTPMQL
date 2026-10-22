using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoMVC.Models
{
    public class Khoa
    {
        [Key]
        public string MaKhoa { get; set; } // Ví dụ: "CNTT", "DK", ...
        
        [Required]
        public string TenKhoa { get; set; } // Ví dụ: "Công nghệ thông tin", "Dầu khí"

        // Quan hệ 1 - N với SinhVien
        public ICollection<SinhVien> SinhViens { get; set; }
    }

    public class SinhVien
    {
        [Key]
        public string MaSinhVien { get; set; } // Mã sinh viên tự động sinh (VD: 2821050199)

        [Required]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; }

        // Khóa ngoại liên kết với Khoa
        [Display(Name = "Khoa")]
        public string MaKhoa { get; set; }
        
        [ForeignKey("MaKhoa")]
        public Khoa Khoa { get; set; }
    }
}