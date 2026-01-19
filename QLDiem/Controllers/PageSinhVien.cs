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


        [HttpGet]
        public ActionResult DangKyLHP()
        {
            var ds = _context.MoLopHocPhans.Include(m => m.MaHpNavigation).ToList();
            if (ds == null)
            {
                TempData["Error1"] = "Không có lớp học phần nào đang mở";
                return View();
            }

            var maSv = HttpContext.Session.GetString("MaSv");

            var dsDaDk = _context.DangKyMonHocs
                .Include(dk => dk.MaLopHpNavigation)
                    .ThenInclude(lhp => lhp.MaHpNavigation)
                .Where(dk =>
                    dk.MaSv == maSv &&
                    _context.MoLopHocPhans.Any(mo =>
                        mo.MaHp == dk.MaLopHpNavigation.MaHp &&
                        mo.HocKy == dk.HocKy &&
                        mo.NamHoc == dk.NamHoc
                    )
                )
                .ToList();

            ViewBag.DsDaDk = dsDaDk;
            return View(ds);
        }

        //XỬ LÝ ĐĂNG KÝ LỚP HỌC PHẦN
        [HttpPost]
        public async Task<IActionResult> DangKyLHP(int maMoLop)
        {
            bool SvDaDk = true;
            var maSv = HttpContext.Session.GetString("MaSv");
            var dsDaDk = _context.DangKyMonHocs
                    .Include(dk => dk.MaLopHpNavigation)
                        .ThenInclude(lhp => lhp.MaHpNavigation)
                    .Where(dk =>
                        dk.MaSv == maSv &&
                        _context.MoLopHocPhans.Any(mo =>
                            mo.MaHp == dk.MaLopHpNavigation.MaHp &&
                            mo.HocKy == dk.HocKy &&
                            mo.NamHoc == dk.NamHoc
                        )
                    )
                    .ToList();

            ViewBag.DsDaDk = dsDaDk;
            var lop = _context.MoLopHocPhans.FirstOrDefault(m => m.MaMoLop == maMoLop);

            var checkLHP = _context.LopHocPhans.FirstOrDefault(l => l.MaHp == lop.MaHp && l.HocKy == lop.HocKy && l.NamHoc == lop.NamHoc);
            if (checkLHP != null)
            {

                var checkSvDk = _context.DangKyMonHocs.FirstOrDefault(dk => dk.MaSv == maSv && dk.MaLopHp == checkLHP.MaLopHp);
                if (checkSvDk == null)
                {
                    SvDaDk = false;
                }
            }
            else
            {
                SvDaDk = false;
            }

            if (SvDaDk)
            {
                TempData["Error"] = "Bạn đã đăng ký lớp học phần này rồi!";
                var dsAfterError = _context.MoLopHocPhans.Include(m => m.MaHpNavigation).ToList();
                return View(dsAfterError);
            }
            if (checkLHP == null)
            {
                await ThemLHP(lop.MaHp, lop.HocKy, lop.NamHoc);
                var checkAfter = _context.LopHocPhans.FirstOrDefault(l => l.MaHp == lop.MaHp && l.HocKy == lop.HocKy && l.NamHoc == lop.NamHoc);
                await AddSVDangKy(maSv, lop.HocKy, lop.NamHoc, DateTime.Now, checkAfter.MaLopHp);
                await UpdateSL(maMoLop);
                TempData["Success"] = "Đăng ký lớp học phần thành công!";
            }
            else
            {
                await AddSVDangKy(maSv, lop.HocKy, lop.NamHoc, DateTime.Now, checkLHP.MaLopHp);
                await UpdateSL(maMoLop);
                TempData["Success"] = "Đăng ký lớp học phần thành công!";
            }
            var ds = _context.MoLopHocPhans
                 .Include(m => m.MaHpNavigation)
                 .ToList();

            return View(ds);

        }
        //Thêm một lớp học phần mới khi chưa có sinh viên nào học, khi có sinh viên đăng ký
        //nà chưa có lớp sẽ thêm lớp mới vào
        public async Task ThemLHP(string maHp, int hocKy, string namHoc)
        {
            string maLopHp;
            do
            {
                maLopHp = "LHP" + Random.Shared.Next(100000, 999999);
            }
            while (await _context.LopHocPhans
                         .AnyAsync(l => l.MaLopHp == maLopHp));
            var hpMoi = new LopHocPhan
            {
                MaLopHp = maLopHp,
                MaHp = maHp,
                HocKy = hocKy,
                NamHoc = namHoc

            };

            _context.LopHocPhans.Add(hpMoi);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSL(int maMoLop)
        {
            var moLop = _context.MoLopHocPhans.FirstOrDefault(m => m.MaMoLop == maMoLop);
            if (moLop != null)
            {
                moLop.SoLuongHienTai += 1;
                _context.MoLopHocPhans.Update(moLop);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddSVDangKy(string maSv, int hocKy, string namHoc, DateTime dateNow, string maLopHp)
        {
            var svDangKy = new DangKyMonHoc
            {
                MaSv = maSv,
                MaLopHp = maLopHp,
                HocKy = hocKy,
                NamHoc = namHoc,
                NgayDangKy = dateNow
            };
            _context.DangKyMonHocs.Add(svDangKy);
            await _context.SaveChangesAsync();
        }


        //XỬ LÝ HỦY LỚP HỌC PHẦN
        [HttpPost]
        public async Task<IActionResult> HuyDangKyLHP(int maDangKy)
        {
            var maSv = HttpContext.Session.GetString("MaSv");

            var dangKy = await _context.DangKyMonHocs
                .FirstOrDefaultAsync(dk => dk.MaDk == maDangKy && dk.MaSv == maSv);

            if (dangKy == null)
            {
                TempData["Error"] = "Không tìm thấy đăng ký hợp lệ!";
                return RedirectToAction("DangKyLHP");
            }

            var lopHp = await _context.LopHocPhans.FindAsync(dangKy.MaLopHp);

            if (lopHp != null)
            {
                var moLop = await _context.MoLopHocPhans
                    .FirstOrDefaultAsync(m =>
                        m.MaHp == lopHp.MaHp &&
                        m.HocKy == lopHp.HocKy &&
                        m.NamHoc == lopHp.NamHoc);

                if (moLop != null && moLop.SoLuongHienTai > 0)
                {
                    moLop.SoLuongHienTai--;
                }
            }

            _context.DangKyMonHocs.Remove(dangKy);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Hủy đăng ký lớp học phần thành công!";
            var ds = _context.MoLopHocPhans
               .Include(m => m.MaHpNavigation)
               .ToList();
            return RedirectToAction("DangKyLHP");


        }

        public IActionResult BackIndexSv(String id)
        {
            return RedirectToAction("Index", "PageSinhVien", new { id });



        }


        public async Task<IActionResult> Profile()
        {
            var maSV = HttpContext.Session.GetString("MaSv");
            var sv = await _context.SinhViens
                .FirstOrDefaultAsync(s => s.MaSv == maSV);
            return View(sv);
        }

    }
}
