namespace Demo1Controller.Controllers
{
    using System.Threading.Tasks.Dataflow;
    using Microsoft.AspNetCore.Mvc;

    public class Demo1Controller : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Welcome()
        {
            ViewData["Message"] = "Hello World";
            ViewBag.Hoten="Chu Thien Tu";
            return View();
        }

[HttpPost]
public IActionResult Create(String MaSP, String TenSP,String DonGia )
{
    // Thêm sản phẩm

    TempData["Success"] = "Đã thêm sản phẩm thành công !"+"Mã sản phẩm:"+MaSP+",Tên sản phẩm"+TenSP+"Đơn giá sản phẩm"+DonGia;

    return RedirectToAction("Index");
}
    }
}