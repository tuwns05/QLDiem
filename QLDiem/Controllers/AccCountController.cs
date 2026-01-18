    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using QLDiem.Models;

namespace QLDiem.Controllers
{
    public class AccCountController : Controller
    {
        private readonly QuanLyDiemContext quanLyDiemContext;

        public AccCountController(QuanLyDiemContext quanLyDiemContext)
        {
            this.quanLyDiemContext = quanLyDiemContext;
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(string Username, string Password)
        {


            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin";
                return View();
            }

            var checkTaiKhoan = quanLyDiemContext.TaiKhoans.FirstOrDefault(tk => tk.TenDangNhap == Username && tk.MatKhau == Password);

            if (checkTaiKhoan != null)
            {
                HttpContext.Session.SetString("VaiTro", checkTaiKhoan.VaiTro);
                if (!string.IsNullOrEmpty(checkTaiKhoan.MaSv))
                {
                    HttpContext.Session.SetString("MaSv", checkTaiKhoan.MaSv);
                }
                if (checkTaiKhoan.VaiTro == "admin")
                {
                    return RedirectToAction("Index", "Home");

                }
                else
                {
                    return RedirectToAction("Index", "PageSinhVien", new { id = checkTaiKhoan.MaSv });
                }
            }
            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng";
            return View();

        }
        public IActionResult TestRedirect()
        {
            // Thử chuyển hướng trực tiếp
            return RedirectToAction("Index", "Admin");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "AccCount");

        }

    }
}
