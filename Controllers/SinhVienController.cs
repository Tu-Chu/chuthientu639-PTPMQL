using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DemoMVC.Data;
using DemoMVC.Models;

public class SinhVienController : Controller
{
    private readonly ApplicationDbContext _context;

    public SinhVienController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: SinhVien/Create
    public IActionResult Create()
    {
        // Đổ dữ liệu các Khoa ra DropdownList để nhập liệu[cite: 3]
        ViewData["MaKhoa"] = new SelectList(_context.Khoa, "MaKhoa", "TenKhoa");
        return View();
    }

    // POST: SinhVien/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("HoTen,MaKhoa")] SinhVien sinhVien)
    {
        // Giả sử quy tắc sinh mã cho năm 2028, hệ chính quy (21), ngành học theo mã khoa (ví dụ CNTT tương ứng 05)[cite: 3]
        string namNhapHoc = "28"; // 2 năm cuối của năm 2028
        string heDaoTao = "21";   // Đại học chính quy
        string maNganh = "05";    // Ví dụ mã ngành Công nghệ thông tin

        string tienTo = namNhapHoc + heDaoTao + maNganh; // Ví dụ: "282105"

        // Tìm mã sinh viên lớn nhất hiện tại bắt đầu bằng tiền tố này để lấy số thứ tự tiếp theo
        var svCuoiCung = await _context.SinhVien
            .Where(s => s.MaSinhVien.StartsWith(tienTo))
            .OrderByDescending(s => s.MaSinhVien)
            .FirstOrDefaultAsync();

        int stt = 1;
        if (svCuoiCung != null)
        {
            // Lấy 4 chữ số cuối ra và chuyển thành số nguyên, sau đó cộng thêm 1
            string 4SoCuoiStr = svCuoiCung.MaSinhVien.Substring(svCuoiCung.MaSinhVien.Length - 4);
            if (int.TryParse(4SoCuoiStr, out int sttCuoi))
            {
                stt = sttCuoi + 1;
            }
        }

        // Ghép lại thành mã sinh viên hoàn chỉnh (Định dạng 4 chữ số cuối, ví dụ: 0199 -> 0200)[cite: 3]
        sinhVien.MaSinhVien = tienTo + stt.ToString("D4");

        if (ModelState.IsValid)
        {
            _context.Add(sinhVien);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["MaKhoa"] = new SelectList(_context.Khoa, "MaKhoa", "TenKhoa", sinhVien.MaKhoa);
        return View(sinhVien);
    }
}