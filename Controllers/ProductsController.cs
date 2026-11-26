// Controllers/ProductsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcAjaxCrud.Data;
using MvcAjaxCrud.Models;

namespace MvcAjaxCrud.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // Trang chính chứa bảng và modal
        public IActionResult Index()
        {
            return View();
        }

        // READ: Lấy danh sách sản phẩm
        [HttpGet]
        public async Task<IActionResult> GetAll(string? search, int page = 1, int pageSize = 10)
        {
            var allowedPageSizes = new[] { 5, 10, 20, 50 };
            if (!allowedPageSizes.Contains(pageSize))
            {
                pageSize = 10;
            }

            var query = _context.Products.AsNoTracking();
            var searchTerm = search?.Trim();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(product => product.Name.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Clamp(page, 1, Math.Max(totalPages, 1));

            var items = await query
                .OrderByDescending(product => product.Id)
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

        // READ ONE: Lấy 1 sản phẩm để sửa
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Không tìm thấy sản phẩm." });

            return Json(product);
        }

        // CREATE: Thêm sản phẩm mới
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] Product product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Thêm thành công", data = product });
        }

        // UPDATE: Cập nhật sản phẩm
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] Product product)
        {
            if (id != product.Id)
                return BadRequest(new { message = "ID không khớp." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _context.Products.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy sản phẩm." });

            existing.Name = product.Name;
            existing.Price = product.Price;
            existing.Stock = product.Stock;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công", data = existing });
        }

        // DELETE: Xóa sản phẩm
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Không tìm thấy sản phẩm." });

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa thành công" });
        }
    }
}