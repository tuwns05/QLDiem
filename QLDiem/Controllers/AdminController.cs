using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using QLDiem.Models;

namespace QLDiem.Controllers
{
    public class AdminController : Controller
    {
        private readonly QuanLyDiemContext _context;

        public AdminController(QuanLyDiemContext context)
        {
            _context = context;
        }

        // GET: Admin
        // Nếu truyền maSv hoặc maHp (maLopHP) sẽ bật form nhập điểm bên view
        public async Task<IActionResult> Index(string maSv, string maLopHp)
        {
            if (!string.IsNullOrEmpty(maSv) || !string.IsNullOrEmpty(maLopHp))
            {
                // nếu maHp chính là MaLopHP (lớp học phần), tìm LopHocPhan theo khoá này
                var sinhVien = !string.IsNullOrEmpty(maSv) ? await _context.SinhViens.FindAsync(maSv) : null;
                var lopHocPhan = !string.IsNullOrEmpty(maLopHp) ? await _context.LopHocPhans.FindAsync(maLopHp) : null;

                ViewBag.ShowNhapDiem = true;
                ViewBag.DiemModel = new Diem
                {
                    MaSv = maSv,
                    MaLopHp = maLopHp,
                    MaSvNavigation = sinhVien,
                    MaLopHpNavigation = lopHocPhan
                };
            }

            // nếu có session lưu MaLopHP thì tự động mở FillLopHP
            var luuMaHp = HttpContext.Session.GetString("LuuMaHp");
            if (!string.IsNullOrEmpty(luuMaHp))
            {
                return await FillLopHP(luuMaHp);
            }

            return View();
        }

        // GET: Admin/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var diem = await _context.Diems
                .Include(d => d.MaLopHpNavigation)
                .Include(d => d.MaSvNavigation)
                .FirstOrDefaultAsync(m => m.MaDiem == id);

            if (diem == null) return NotFound();

            return View(diem);
        }

        // GET: Admin/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var diem = await _context.Diems
                .Include(d => d.MaLopHpNavigation)
                .Include(d => d.MaSvNavigation)
                .FirstOrDefaultAsync(m => m.MaDiem == id);

            if (diem == null) return NotFound();

            return View(diem);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var diem = await _context.Diems.FindAsync(id);
            if (diem != null)
            {
                _context.Diems.Remove(diem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DiemExists(int id)
        {
            return _context.Diems.Any(e => e.MaDiem == id);
        }

        // GET: Admin/FillLopHP?MaLopHP=LH001
        public async Task<IActionResult> FillLopHP(string MaLopHP)
        {
            if (string.IsNullOrWhiteSpace(MaLopHP))
            {
                ViewBag.Message = "Vui lòng chọn lớp học phần";
                return View("Index");
            }

            // Lấy tên học phần (từ LopHocPhan -> HocPhan) để hiển thị header
            ViewBag.tenHP = await (from l in _context.LopHocPhans
                                   join hp in _context.HocPhans on l.MaHp equals hp.MaHp
                                   where l.MaLopHp == MaLopHP
                                   select hp.TenHp)
                                  .FirstOrDefaultAsync() ?? MaLopHP;

          
            var ds = await (from dk in _context.DangKyMonHocs
                            where dk.MaLopHp == MaLopHP
                            join sv in _context.SinhViens on dk.MaSv equals sv.MaSv
                            join lhp in _context.LopHocPhans on dk.MaLopHp equals lhp.MaLopHp
                            join d in _context.Diems.Where(x => x.MaLopHp == MaLopHP)
                                on sv.MaSv equals d.MaSv into dg
                            from diem in dg.DefaultIfEmpty()
                            select new Diem
                            {
                                MaDiem = diem != null ? diem.MaDiem : 0,
                                MaSv = sv.MaSv,
                                MaLopHp = MaLopHP,
                                DiemQt = diem != null ? diem.DiemQt : (double?)null,
                                DiemCk = diem != null ? diem.DiemCk : (double?)null,
                                DiemTk = diem != null ? diem.DiemTk : (double?)null,
                                MaSvNavigation = sv,
                                MaLopHpNavigation = lhp
                            }).ToListAsync();

            // Optionally store current MaLopHP into session so Index can auto-open later
            HttpContext.Session.SetString("LuuMaHp", MaLopHP);

            return View("Index", ds);
        }

        //NHẬP ĐIỂM
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NhapDiem(Diem d)
        {
            try
            {
                var lopHocPhan = await _context.LopHocPhans
                                .Include(l => l.MaHpNavigation)
                                .FirstOrDefaultAsync(l => l.MaLopHp == d.MaLopHp);

                double heSo = 1.0; // Giá trị mặc định

                // Kiểm tra nhiều cấp độ null
                if (lopHocPhan?.MaHpNavigation?.HeSo != null)
                {
                    heSo = lopHocPhan.MaHpNavigation.HeSo.Value;
                }

                d.DiemTk = tinhDiemTK(heSo, d.DiemQt.Value, d.DiemCk.Value);
                _context.Add(d);
                await _context.SaveChangesAsync();
               // HttpContext.Session.SetString("LuuMaHp", d.MaLopHp);
                return RedirectToAction("FillLopHP", new { MaLopHP = d.MaLopHp });

            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Lỗi khi lưu điểm: " + ex.Message);
                return RedirectToAction("FillLopHP", new { MaLopHP = d.MaLopHp });
            }

        }


        // Trả về danh sách các lớp học phần (MaLopHP) thuộc học kỳ & năm học
        [HttpGet]
        public async Task<IActionResult> GetLopHocPhanByHocKyNamHoc(int hocKy, string namHoc)
        {
            try
            {
                // Kiểm tra tham số
                if (hocKy <= 0 || string.IsNullOrWhiteSpace(namHoc))
                {
                    return Json(new List<object>());
                }

                var data = await _context.LopHocPhans
                    .Include(l => l.MaHpNavigation)
                    .Where(l => l.HocKy == hocKy && l.NamHoc == namHoc)
                    .OrderBy(l => l.MaLopHp)
                    .Select(l => new
                    {
                        maLopHp = l.MaLopHp,
                        tenHp = l.MaHpNavigation != null ? l.MaHpNavigation.TenHp : "Không có tên"
                    })
                    .ToListAsync();

                return Json(data);
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new List<object>());
            }
        }

        // HÀM TÍNH ĐIỂM TỔNG KẾT
        public double tinhDiemTK(double heSo, double diemQT, double diemCK)
        {
            
            double tyLeQT = heSo / (1 + heSo);
            double tyLeCK = 1 / (1 + heSo);

            double diem = diemQT * tyLeQT + diemCK * tyLeCK;
            return Math.Round(diem, 2);
        }
    }
}
