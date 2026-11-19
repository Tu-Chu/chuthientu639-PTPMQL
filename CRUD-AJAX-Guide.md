# Hướng dẫn CRUD AJAX với ASP.NET Core MVC

Bài thực hành dùng model `Employee`, Entity Framework Core, SQLite, controller MVC trả JSON và `fetch` ở trình duyệt. Mã nguồn nằm trong `Models/Employee.cs`, `Models/EmployeeInput.cs`, `Data/ApplicationDbContext.cs`, `Controllers/EmployeesController.cs`, `Views/Employees/Index.cshtml` và `wwwroot/js/employees.js`.

## 1. Model dữ liệu

`Employee` là entity được EF Core lưu vào bảng `Employees`. `EmployeeInput` là dữ liệu đầu vào riêng cho thao tác thêm/sửa, có Data Annotations để kiểm tra họ tên, email, phòng ban và lương. Tách input khỏi entity giúp client không thể tự gửi `Id` để thay đổi khóa chính.

## 2. Kết nối SQLite

`appsettings.json` có connection string `DefaultConnection` trỏ tới file `App.db`. `Program.cs` đăng ký `ApplicationDbContext`; context này chứa cả dữ liệu Person hiện có và `DbSet<Employee>`. Trong bài demo, `EnsureCreatedAsync()` tạo database và bảng khi khởi chạy lần đầu.

> `EnsureCreated` phù hợp để học nhanh trên database mới. Khi cấu trúc bảng thay đổi hoặc ứng dụng đã có dữ liệu cần giữ, hãy chuyển sang EF Core Migrations thay vì dựa vào `EnsureCreated`.

## 3. Controller cung cấp API JSON

| HTTP | Action | Công việc |
| --- | --- | --- |
| GET | `/Employees/GetAll` | Đọc danh sách |
| GET | `/Employees/GetById?id=1` | Đọc một nhân viên |
| POST | `/Employees/Create` | Thêm nhân viên |
| PUT | `/Employees/Update?id=1` | Cập nhật nhân viên |
| DELETE | `/Employees/Delete?id=1` | Xóa nhân viên |

Controller kiểm tra `ModelState`, trả `404` khi không tìm thấy bản ghi, và dùng async EF Core để không chặn luồng xử lý trong lúc truy cập database.

## 4. View và AJAX

View tạo bảng và dialog thêm/sửa. Các URL được sinh bằng `Url.Action`, không hard-code đường dẫn. `employees.js` dùng `fetch` để gọi API, cập nhật bảng mà không tải lại trang, và ghi nội dung người dùng bằng `textContent` để tránh chèn HTML không an toàn.

Form có antiforgery token. JavaScript gửi token trong header `RequestVerificationToken`; các action POST, PUT, DELETE xác thực token bằng `[ValidateAntiForgeryToken]`.

## 5. Chạy thử

Từ thư mục workspace:

```powershell
dotnet run --project DemoMVC/DemoMVC.csproj
```

Mở `/Employees`. Thử lần lượt thêm nhân viên, sửa bản ghi, tải lại trang để kiểm tra dữ liệu SQLite còn nguyên, rồi xóa bản ghi. Thử nhập email sai hoặc lương âm để xem validation phía trình duyệt và máy chủ.

## 6. Luồng dữ liệu

1. Trình duyệt gửi `fetch` tới action tương ứng.
2. ASP.NET Core bind JSON vào `EmployeeInput` và kiểm tra Data Annotations.
3. Controller truy vấn hoặc cập nhật `ApplicationDbContext`.
4. EF Core lưu thay đổi xuống SQLite.
5. Controller trả JSON; JavaScript hiển thị thông báo và tải lại riêng phần danh sách.