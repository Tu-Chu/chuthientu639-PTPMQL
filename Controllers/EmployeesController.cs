using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcAjaxModalCrud.Data;
using MvcAjaxModalCrud.Models;

namespace MvcAjaxModalCrud.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly AppDbContext _context;

        public EmployeesController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // READ: Lấy danh sách nhân viên
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.Employees
                .OrderByDescending(e => e.Id)
                .ToListAsync();
            return Json(list);
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
        public async Task<IActionResult> Create([FromBody] Employee emp)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Employees.Add(emp);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Thêm nhân viên thành công", data = emp });
        }

        // UPDATE: Cập nhật từ Modal
        [HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody] Employee emp)
        {
            if (id != emp.Id)
                return BadRequest(new { message = "ID không hợp lệ." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _context.Employees.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy nhân viên." });

            existing.FullName = emp.FullName;
            existing.Email = emp.Email;
            existing.Department = emp.Department;
            existing.Salary = emp.Salary;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công", data = existing });
        }

        // DELETE: Xóa sau khi xác nhận trên Delete Modal
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp == null)
                return NotFound(new { message = "Không tìm thấy nhân viên." });

            _context.Employees.Remove(emp);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa nhân viên" });
        }
    }
}