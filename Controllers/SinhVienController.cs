using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DemoMVC.Data;
using DemoMVC.Models;

[Authorize]
public class SinhVienController : Controller
{
    private readonly ApplicationDbContext _context;

    public SinhVienController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: SinhVien
    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 10)
    {
        var allowedPageSizes = new[] { 5, 10, 20, 50 };
        if (!allowedPageSizes.Contains(pageSize)) pageSize = 10;

        var query = _context.SinhVien
            .Include(sinhVien => sinhVien.Khoa)
            .AsNoTracking();
        var searchTerm = search?.Trim();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(sinhVien =>
                sinhVien.MaSinhVien.Contains(searchTerm) ||
                sinhVien.HoTen.Contains(searchTerm) ||
                sinhVien.MaKhoa.Contains(searchTerm) ||
                sinhVien.Khoa.TenKhoa.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        page = Math.Clamp(page, 1, Math.Max(totalPages, 1));

        var items = await query
            .OrderByDescending(sinhVien => sinhVien.MaSinhVien)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return View(new PagedResult<SinhVien>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            Search = searchTerm ?? string.Empty
        });
    }

    // GET: SinhVien/Create
    public IActionResult Create()
    {
        // Äá»• dá»¯ liá»‡u cÃ¡c Khoa ra DropdownList Ä‘á»ƒ nháº­p liá»‡u[cite: 3]
        ViewData["MaKhoa"] = new SelectList(_context.Khoa, "MaKhoa", "TenKhoa");
        return View();
    }

    // POST: SinhVien/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("HoTen,MaKhoa")] SinhVien sinhVien)
    {
        // Giáº£ sá»­ quy táº¯c sinh mÃ£ cho nÄƒm 2028, há»‡ chÃ­nh quy (21), ngÃ nh há»c theo mÃ£ khoa (vÃ­ dá»¥ CNTT tÆ°Æ¡ng á»©ng 05)[cite: 3]
        string namNhapHoc = "28"; // 2 nÄƒm cuá»‘i cá»§a nÄƒm 2028
        string heDaoTao = "21"; // Äáº¡i há»c chÃ­nh quy
        string maNganh = "05";    // VÃ­ dá»¥ mÃ£ ngÃ nh CÃ´ng nghá»‡ thÃ´ng tin

        string tienTo = namNhapHoc + heDaoTao + maNganh; // VÃ­ dá»¥: "282105"

        // TÃ¬m mÃ£ sinh viÃªn lá»›n nháº¥t hiá»‡n táº¡i báº¯t Ä‘áº§u báº±ng tiá»n tá»‘ nÃ y Ä‘á»ƒ láº¥y sá»‘ thá»© tá»± tiáº¿p theo
        var svCuoiCung = await _context.SinhVien
            .Where(s => s.MaSinhVien.StartsWith(tienTo))
            .OrderByDescending(s => s.MaSinhVien)
            .FirstOrDefaultAsync();

        int stt = 1;
        if (svCuoiCung != null)
        {
            // Láº¥y 4 chá»¯ sá»‘ cuá»‘i ra vÃ  chuyá»ƒn thÃ nh sá»‘ nguyÃªn, sau Ä‘Ã³ cá»™ng thÃªm 1
            string lastFourDigits = svCuoiCung.MaSinhVien.Substring(svCuoiCung.MaSinhVien.Length - 4);
            if (int.TryParse(lastFourDigits, out int sttCuoi))
            {
                stt = sttCuoi + 1;
            }
        }

        // GhÃ©p láº¡i thÃ nh mÃ£ sinh viÃªn hoÃ n chá»‰nh (Äá»‹nh dáº¡ng 4 chá»¯ sá»‘ cuá»‘i, vÃ­ dá»¥: 0199 -> 0200)[cite: 3]
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
