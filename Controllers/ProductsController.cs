// Controllers/ProductsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcAjaxCrud.Data;
using MvcAjaxCrud.Models;

namespace MvcAjaxCrud.Controllers
{
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
        public async Task<IActionResult> GetAll()
        {
            var products = await _context.Products
                .OrderByDescending(p => p.Id)
                .ToListAsync();
            return Json(products);
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