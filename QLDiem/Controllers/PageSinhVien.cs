using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDiem.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QLDiem.Controllers
{
    public class PageSinhVien : Controller
    {
        private readonly QuanLyDiemContext _context;

        public PageSinhVien(QuanLyDiemContext context)
        {
            _context = context;
        }

        // GET: PageSinhVien/Index?id=SV01
        public async Task<ActionResult> Index(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            // ===============================
            // 1. THÔNG TIN SINH VIÊN
            // ===============================
            var sinhVien = await _context.SinhViens
                .FirstOrDefaultAsync(m => m.MaSv == id);

            if (sinhVien == null)
                return NotFound();

            HttpContext.Session.SetString("HoTen", sinhVien.HoTen);
            HttpContext.Session.SetString("Lop", sinhVien.Lop);

            // ===============================
            // 2. BẢNG ĐIỂM CHI TIẾT
            // ===============================
            var diemSV = await (
                from dk in _context.DangKyMonHocs
                where dk.MaSv == id

                join lhp in _context.LopHocPhans
                    on dk.MaLopHp equals lhp.MaLopHp

                join hp in _context.HocPhans
                    on lhp.MaHp equals hp.MaHp

                join d in _context.Diems
                    on new { dk.MaSv, dk.MaLopHp }
                    equals new { d.MaSv, d.MaLopHp }
                    into dg
                from diem in dg.DefaultIfEmpty()

                select new DiemSinhVienVm
                {
                    MaSv = dk.MaSv,

                    MaHp = hp.MaHp,
                    TenHp = hp.TenHp,
                    SoTinChi = (int)hp.SoTinChi,

                    HocKy = lhp.HocKy,
                    NamHoc = lhp.NamHoc,

                    DiemQt = diem != null ? diem.DiemQt : null,
                    DiemCk = diem != null ? diem.DiemCk : null,
                    DiemTk = diem != null ? diem.DiemTk : null,

                    KetQua = diem != null && diem.DiemTk >= 4.0
                }
            ).ToListAsync();

            // ===============================
            // 3. GPA (HỌC KỲ + TÍCH LŨY)
            // ===============================
            var gpaList = await _context.Gpas
                .Where(g => g.MaSv == id)
                .OrderBy(g => g.NamHoc)
                .ThenBy(g => g.HocKy)
                .ToListAsync();

            // Gửi GPA sang View
            ViewBag.GPA = gpaList;

            // ===============================
            // 4. TRẢ VIEW
            // ===============================
            return View(diemSV);
        }
    }
}
