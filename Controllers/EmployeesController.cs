using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DemoMVC.Data;
using MvcAjaxModalCrud.Models;

namespace MvcAjaxModalCrud.Controllers
{
    [Authorize]
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

        // READ: Láº¥y danh sÃ¡ch nhÃ¢n viÃªn
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

        // READ ONE: Láº¥y dá»¯ liá»‡u nhÃ¢n viÃªn Ä‘á»ƒ Ä‘á»• vÃ o Modal Sá»­a
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp == null)
                return NotFound(new { message = "KhÃ´ng tÃ¬m tháº¥y nhÃ¢n viÃªn." });

            return Json(emp);
        }

        // CREATE: ThÃªm má»›i tá»« Modal
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

            return Ok(new { message = "ThÃªm nhÃ¢n viÃªn thÃ nh cÃ´ng", data = emp });
        }

        // UPDATE: Cáº­p nháº­t tá»« Modal
        [HttpPut]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, [FromBody] EmployeeInput input)
        {
            if (!ModelState.IsValid)
                return ValidationError();

            var existing = await _context.Employees.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "KhÃ´ng tÃ¬m tháº¥y nhÃ¢n viÃªn." });

            existing.FullName = input.FullName;
            existing.Email = input.Email;
            existing.Department = input.Department;
            existing.Salary = input.Salary;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cáº­p nháº­t thÃ nh cÃ´ng", data = existing });
        }

        // DELETE: XÃ³a sau khi xÃ¡c nháº­n trÃªn Delete Modal
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp == null)
                return NotFound(new { message = "KhÃ´ng tÃ¬m tháº¥y nhÃ¢n viÃªn." });

            _context.Employees.Remove(emp);
            await _context.SaveChangesAsync();

            return Ok(new { message = "ÄÃ£ xÃ³a nhÃ¢n viÃªn" });
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
                message = "Dá»¯ liá»‡u chÆ°a há»£p lá»‡. Vui lÃ²ng kiá»ƒm tra cÃ¡c trÆ°á»ng bÃªn dÆ°á»›i.",
                errors
            });
        }
    }
}
