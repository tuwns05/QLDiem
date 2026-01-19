    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
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
       
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "AccCount");
        }
    
        public async Task<IActionResult> Profile()
        {
            var maSV = HttpContext.Session.GetString("MaSv");
            var sv = await quanLyDiemContext.SinhViens
                .FirstOrDefaultAsync(s => s.MaSv == maSV);
            return View(sv);
        }

        public IActionResult EditPassW()
        {
            var maSV = HttpContext.Session.GetString("MaSv");

            if (string.IsNullOrEmpty(maSV))
            {
                TempData["Error"] = "Vui lòng đăng nhập";
                return RedirectToAction("Login");
            }

            return View(new TaiKhoan());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPassW(TaiKhoan model)
        {
            // Loại bỏ các field không cần validate
            ModelState.Remove("TenDangNhap");
            ModelState.Remove("VaiTro");
            ModelState.Remove("MaSv");
            ModelState.Remove("MatKhau");

            var maSV = HttpContext.Session.GetString("MaSv");

            if (string.IsNullOrEmpty(maSV))
            {
                TempData["Error"] = "Vui lòng đăng nhập";
                return RedirectToAction("Login");
            }

            // Validation thủ công
            if (string.IsNullOrEmpty(model.CurrentPassword))
            {
                ModelState.AddModelError("CurrentPassword", "Vui lòng nhập mật khẩu hiện tại");
            }

            if (string.IsNullOrEmpty(model.NewPassword))
            {
                ModelState.AddModelError("NewPassword", "Vui lòng nhập mật khẩu mới");
            }
            else if (model.NewPassword.Length < 6)
            {
                ModelState.AddModelError("NewPassword", "Mật khẩu mới phải có ít nhất 6 ký tự");
            }

            if (string.IsNullOrEmpty(model.ConfirmPassword))
            {
                ModelState.AddModelError("ConfirmPassword", "Vui lòng xác nhận mật khẩu mới");
            }
            else if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Mật khẩu xác nhận không khớp");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Lấy tài khoản
            var tk = await quanLyDiemContext.TaiKhoans
                .FirstOrDefaultAsync(t => t.MaSv == maSV);

            if (tk == null)
            {
                TempData["Error"] = "Không tìm thấy tài khoản";
                return RedirectToAction("Profile");
            }

            // Kiểm tra mật khẩu hiện tại
            if (tk.MatKhau != model.CurrentPassword)
            {
                ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng");
                return View(model);
            }

            try
            {
                // Cập nhật mật khẩu mới
                tk.MatKhau = model.NewPassword;
                await quanLyDiemContext.SaveChangesAsync();

                TempData["Success"] = "Đổi mật khẩu thành công!";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                return View(model);
            }
        }

        public async Task<IActionResult> EditTTSV()
        {
            var maSV = HttpContext.Session.GetString("MaSv");
            var sv = await quanLyDiemContext.SinhViens
                .FirstOrDefaultAsync(s => s.MaSv == maSV);
            if (sv == null)
                return NotFound();
            return View(sv);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTTSV(SinhVien model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Lấy mã SV đang đăng nhập
            var maSv = HttpContext.Session.GetString("MaSv");

            if (string.IsNullOrEmpty(maSv))
                return RedirectToAction("Login");

            // Không cho sửa thông tin người khác
            if (maSv != model.MaSv)
                return Unauthorized();

            // Lấy sinh viên trong DB
            var sv = await quanLyDiemContext.SinhViens
                .FirstOrDefaultAsync(s => s.MaSv == maSv);

            if (sv == null)
                return NotFound("Không tìm thấy sinh viên");

            // Cập nhật thông tin
            sv.HoTen = model.HoTen;
            sv.Lop = model.Lop;
            sv.GioiTinh = model.GioiTinh;
            sv.NgaySinh = model.NgaySinh;

            await quanLyDiemContext.SaveChangesAsync();

            // Cập nhật lại Session HoTen (để layout hiện đúng)
            HttpContext.Session.SetString("HoTen", sv.HoTen ?? "");

            TempData["Success"] = "Cập nhật thông tin thành công";
            return RedirectToAction("Profile");
        }
    }
}
