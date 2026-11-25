using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DemoMVC.Data;
using MvcAjaxModalCrud.Models;

namespace MvcAjaxModalCrud.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // READ: Lấy danh sách nhân viên
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var allowedPageSizes = new[] { 5, 10, 20, 50 };
            if (!allowedPageSizes.Contains(pageSize))
            {
                pageSize = 10;
            }

            var query = _context.Employees.AsNoTracking();
            var searchTerm = search?.Trim();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var pattern = $"%{searchTerm.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_")}%";
                query = query.Where(employee =>
                    EF.Functions.Like(employee.FullName, pattern, "\\") ||
                    EF.Functions.Like(employee.Email, pattern, "\\") ||
                    EF.Functions.Like(employee.Department, pattern, "\\"));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Clamp(page, 1, Math.Max(totalPages, 1));

            var items = await query
                .OrderByDescending(e => e.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Json(new
            {
                items,
                totalCount,
                page,
                pageSize,
                totalPages,
                search = searchTerm ?? string.Empty
            });
        }

        // READ ONE: Lấy dữ liệu nhân viên để đổ vào Modal Sửa
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp == null)
                return NotFound(new { message = "Không tìm thấy nhân viên." });

            return Json(emp);
        }

        // CREATE: Thêm mới từ Modal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] EmployeeInput input)
        {
            if (!ModelState.IsValid)
                return ValidationError();

            var emp = new Employee
            {
                FullName = input.FullName,
                Email = input.Email,
                Department = input.Department,
                Salary = input.Salary
            };
            _context.Employees.Add(emp);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Thêm nhân viên thành công", data = emp });
        }

        // UPDATE: Cập nhật từ Modal
        [HttpPut]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, [FromBody] EmployeeInput input)
        {
            if (!ModelState.IsValid)
                return ValidationError();

            var existing = await _context.Employees.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy nhân viên." });

            existing.FullName = input.FullName;
            existing.Email = input.Email;
            existing.Department = input.Department;
            existing.Salary = input.Salary;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công", data = existing });
        }

        // DELETE: Xóa sau khi xác nhận trên Delete Modal
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp == null)
                return NotFound(new { message = "Không tìm thấy nhân viên." });

            _context.Employees.Remove(emp);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa nhân viên" });
        }

        private IActionResult ValidationError()
        {
            var errors = ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value!.Errors
                        .Select(error => error.ErrorMessage)
                        .ToArray());

            return BadRequest(new
            {
                message = "Dữ liệu chưa hợp lệ. Vui lòng kiểm tra các trường bên dưới.",
                errors
            });
        }
    }
}