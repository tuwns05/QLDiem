using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLDiem.Models;

namespace QLDiem.Controllers
{
    public class LopHocPhansController : Controller
    {
        private readonly QuanLyDiemContext _context;

        public LopHocPhansController(QuanLyDiemContext context)
        {
            _context = context;
        }

        // GET: LopHocPhans
        public async Task<IActionResult> Index(int? hocKy, string namHoc)
        {
            var query = _context.LopHocPhans
                .Include(l => l.MaHpNavigation)
                .Include(l => l.DangKyMonHocs)
                    .ThenInclude(d => d.MaSvNavigation)
                .AsQueryable();

            // Lọc theo học kỳ
            if (hocKy.HasValue)
            {
                query = query.Where(l => l.HocKy == hocKy.Value);
            }

            // Lọc theo năm học
            if (!string.IsNullOrEmpty(namHoc))
            {
                query = query.Where(l => l.NamHoc == namHoc);
            }

            // Lấy danh sách năm học để hiển thị trong dropdown
            var namHocList = await _context.LopHocPhans
                .Select(l => l.NamHoc)
                .Distinct()
                .OrderByDescending(n => n)
                .ToListAsync();

            ViewBag.NamHocList = namHocList;
            ViewBag.HocKy = hocKy;
            ViewBag.NamHoc = namHoc;

            var result = await query
                .OrderByDescending(l => l.NamHoc)
                .ThenByDescending(l => l.HocKy)
                .ToListAsync();

            return View(result);
        }

        // GET: LopHocPhans/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var lopHocPhan = await _context.LopHocPhans
                .Include(l => l.MaHpNavigation)
                .Include(l => l.DangKyMonHocs)
                    .ThenInclude(d => d.MaSvNavigation)
                .Include(l => l.Diems)
                    .ThenInclude(d => d.MaSvNavigation)
                .FirstOrDefaultAsync(m => m.MaLopHp == id);

            if (lopHocPhan == null)
            {
                return NotFound();
            }

            return View(lopHocPhan);
        }

        // GET: LopHocPhans/Create
        public IActionResult Create()
        {
            ViewData["MaHp"] = new SelectList(_context.HocPhans, "MaHp", "TenHp");
            return View();
        }

        // POST: LopHocPhans/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaLopHp,MaHp,HocKy,NamHoc")] LopHocPhan lopHocPhan)
        {
            ModelState.Remove("MaHpNavigation");
            // Validation
            if (string.IsNullOrWhiteSpace(lopHocPhan.MaLopHp))
                ModelState.AddModelError("MaLopHp", "Mã lớp học phần không được để trống");

            if (string.IsNullOrWhiteSpace(lopHocPhan.MaHp))
                ModelState.AddModelError("MaHp", "Vui lòng chọn học phần");

            if (lopHocPhan.HocKy < 1 || lopHocPhan.HocKy > 3)
                ModelState.AddModelError("HocKy", "Học kỳ phải từ 1 đến 3");

            if (string.IsNullOrWhiteSpace(lopHocPhan.NamHoc))
                ModelState.AddModelError("NamHoc", "Năm học không được để trống");
            else
            {
                // Validate format năm học
                var parts = lopHocPhan.NamHoc.Split('-');
                if (parts.Length != 2 ||
                    !int.TryParse(parts[0], out int y1) ||
                    !int.TryParse(parts[1], out int y2) ||
                    y2 != y1 + 1)
                {
                    ModelState.AddModelError("NamHoc", "Năm học phải có định dạng YYYY-YYYY (VD: 2023-2024)");
                }
            }

            // Kiểm tra trùng mã
            if (!string.IsNullOrWhiteSpace(lopHocPhan.MaLopHp))
            {
                var exists = await _context.LopHocPhans
                    .AnyAsync(l => l.MaLopHp == lopHocPhan.MaLopHp);

                if (exists)
                {
                    ModelState.AddModelError("MaLopHp", "Mã lớp học phần đã tồn tại");
                }
            }

            // Kiểm tra học phần có tồn tại
            if (!string.IsNullOrWhiteSpace(lopHocPhan.MaHp))
            {
                var hocPhanExists = await _context.HocPhans
                    .AnyAsync(h => h.MaHp == lopHocPhan.MaHp);

                if (!hocPhanExists)
                {
                    ModelState.AddModelError("MaHp", "Học phần không tồn tại");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(lopHocPhan);
                    await _context.SaveChangesAsync();

                    var hocPhan = await _context.HocPhans
                        .FirstOrDefaultAsync(h => h.MaHp == lopHocPhan.MaHp);

                    TempData["SuccessMessage"] =
                        $"Đã thêm lớp học phần {lopHocPhan.MaLopHp} - {hocPhan?.TenHp} thành công!";

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    var innerMessage = ex.InnerException?.Message ?? ex.Message;

                    if (innerMessage.Contains("FOREIGN KEY"))
                    {
                        ModelState.AddModelError("", "Học phần được chọn không tồn tại");
                    }
                    else if (innerMessage.Contains("PRIMARY KEY") || innerMessage.Contains("UNIQUE"))
                    {
                        ModelState.AddModelError("MaLopHp", "Mã lớp học phần đã tồn tại");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Không thể lưu. Vui lòng thử lại.");
                    }
                }
            }

            ViewData["MaHp"] = new SelectList(_context.HocPhans, "MaHp", "TenHp", lopHocPhan.MaHp);
            return View(lopHocPhan);
        }

        // GET: LopHocPhans/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var lopHocPhan = await _context.LopHocPhans.FindAsync(id);
            if (lopHocPhan == null)
            {
                return NotFound();
            }

            ViewData["MaHp"] = new SelectList(_context.HocPhans, "MaHp", "TenHp", lopHocPhan.MaHp);
            return View(lopHocPhan);
        }

        // POST: LopHocPhans/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaLopHp,MaHp,HocKy,NamHoc")] LopHocPhan lopHocPhan)
        {
            ModelState.Remove("MaHpNavigation");
            if (id != lopHocPhan.MaLopHp)
            {
                return NotFound();
            }

            // Validation
            if (string.IsNullOrWhiteSpace(lopHocPhan.MaHp))
                ModelState.AddModelError("MaHp", "Vui lòng chọn học phần");

            if (lopHocPhan.HocKy < 1 || lopHocPhan.HocKy > 3)
                ModelState.AddModelError("HocKy", "Học kỳ phải từ 1 đến 3");

            if (string.IsNullOrWhiteSpace(lopHocPhan.NamHoc))
                ModelState.AddModelError("NamHoc", "Năm học không được để trống");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lopHocPhan);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật lớp học phần thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LopHocPhanExists(lopHocPhan.MaLopHp))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewData["MaHp"] = new SelectList(_context.HocPhans, "MaHp", "TenHp", lopHocPhan.MaHp);
            return View(lopHocPhan);
        }

        // GET: LopHocPhans/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var lopHocPhan = await _context.LopHocPhans
                .Include(l => l.MaHpNavigation)
                .FirstOrDefaultAsync(m => m.MaLopHp == id);

            if (lopHocPhan == null)
            {
                return NotFound();
            }

            return View(lopHocPhan);
        }

        // POST: LopHocPhans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var lopHocPhan = await _context.LopHocPhans.FindAsync(id);

            if (lopHocPhan != null)
            {
                // Kiểm tra ràng buộc với bảng DangKyMonHoc
                var hasDangKy = await _context.DangKyMonHocs
                    .AnyAsync(d => d.MaLopHp == id);

                // Kiểm tra ràng buộc với bảng Diem
                var hasDiem = await _context.Diems
                    .AnyAsync(d => d.MaLopHp == id);

                if (hasDangKy || hasDiem)
                {
                    TempData["ErrorMessage"] =
                        "Không thể xóa lớp học phần này vì đã có sinh viên đăng ký hoặc có điểm!";
                    return RedirectToAction(nameof(Index));
                }

                _context.LopHocPhans.Remove(lopHocPhan);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa lớp học phần thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: LopHocPhans/AddSinhVien
        public IActionResult AddSinhVien()
        {
            LoadAddSinhVienData();
            return View();
        }

        // POST: LopHocPhans/AddSinhVien
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSinhVien([Bind("MaSv,MaLopHp")] DangKyMonHoc dangKy)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(dangKy.MaSv))
                ModelState.AddModelError("MaSv", "Vui lòng chọn sinh viên");

            if (string.IsNullOrWhiteSpace(dangKy.MaLopHp))
                ModelState.AddModelError("MaLopHp", "Vui lòng chọn lớp học phần");

            // Lấy lớp học phần
            var lopHocPhan = await _context.LopHocPhans
                .Include(l => l.MaHpNavigation)
                .FirstOrDefaultAsync(l => l.MaLopHp == dangKy.MaLopHp);

            if (lopHocPhan == null)
                ModelState.AddModelError("MaLopHp", "Lớp học phần không tồn tại");

            // Lấy sinh viên
            var sinhVien = await _context.SinhViens
                .FirstOrDefaultAsync(s => s.MaSv == dangKy.MaSv);

            if (sinhVien == null)
                ModelState.AddModelError("MaSv", "Sinh viên không tồn tại");

            // Kiểm tra trùng đăng ký (chỉ khi cả 2 đều tồn tại)
            if (lopHocPhan != null && sinhVien != null)
            {
                var daDangKy = await _context.DangKyMonHocs.AnyAsync(d =>
                    d.MaSv == dangKy.MaSv &&
                    d.MaLopHp == dangKy.MaLopHp);

                if (daDangKy)
                    ModelState.AddModelError("MaSv", "Sinh viên đã đăng ký lớp học phần này");
            }

            // Nếu có lỗi
            if (!ModelState.IsValid)
            {
                LoadAddSinhVienData(dangKy);
                return View(dangKy);
            }

            // Copy dữ liệu từ lớp học phần
            dangKy.HocKy = lopHocPhan.HocKy;
            dangKy.NamHoc = lopHocPhan.NamHoc;
            dangKy.NgayDangKy = DateTime.Now;

            // Lưu
            try
            {
                _context.DangKyMonHocs.Add(dangKy);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    $"Đã thêm sinh viên {sinhVien.HoTen} vào lớp {lopHocPhan.MaLopHp}";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi lưu dữ liệu: " + ex.InnerException?.Message);
                LoadAddSinhVienData(dangKy);
                return View(dangKy);
            }
        }

        // Helper method
        private void LoadAddSinhVienData(DangKyMonHoc? model = null)
        {
            // Hiển thị thông tin đầy đủ cho dropdown
            var lopHocPhans = _context.LopHocPhans
                .Include(l => l.MaHpNavigation)
                .Select(l => new
                {
                    l.MaLopHp,
                    DisplayText = l.MaLopHp + " - " + l.MaHpNavigation.TenHp +
                                  " (HK" + l.HocKy + " - " + l.NamHoc + ")"
                })
                .ToList();

            ViewData["MaLopHp"] = new SelectList(
                lopHocPhans,
                "MaLopHp",
                "DisplayText",
                model?.MaLopHp
            );

            ViewData["MaSv"] = new SelectList(
                _context.SinhViens.Select(s => new
                {
                    s.MaSv,
                    DisplayText = s.MaSv + " - " + s.HoTen + " (" + s.Lop + ")"
                }),
                "MaSv",
                "DisplayText",
                model?.MaSv
            );
        }

        private bool LopHocPhanExists(string id)
        {
            return _context.LopHocPhans.Any(e => e.MaLopHp == id);
        }

        //Get mở view mở lớp học phần
        public async Task<IActionResult> MoLopHp()
        {
            ViewBag.HocPhans = await _context.HocPhans.ToListAsync();

            return View();
        }
        // Post mở lớp học phần
        [HttpPost]
        public async Task<IActionResult> MoLopHp(MoLopVM ml)
        {
            var hocPhan = await _context.HocPhans.ToListAsync();

            ViewBag.HocPhans = await _context.HocPhans.ToListAsync();
            bool check = await _context.MoLopHocPhans
                .AnyAsync(x => x.HocKy == ml.HocKy && x.NamHoc == ml.NamHoc);
            if (ml.NgayDong < ml.NgayMo)
            {
                TempData["Error"] = "ngày đóng phải lớn hơn ngày mở";
                return View();
            }
            if (ml.NgayMo < DateTime.Now)
            {
                TempData["Error"] = "Không thể mở lớp ngày nhỏ hơn hiện tại";
                return View();
            }
            if (check)
            {
                TempData["Error"] = "học kỳ này đã mở đăng ký";
                return View();

            }
            else
            {
                var dsMoLop = hocPhan.Select(hp => new MoLopHocPhan
                {
                    MaHp = hp.MaHp,
                    HocKy = ml.HocKy,
                    NamHoc = ml.NamHoc,
                    SoLuong = ml.SoLuong,
                    SoLuongHienTai = 0,
                    NgayMo = ml.NgayMo,
                    NgayDong = ml.NgayDong
                }).ToList();
                _context.MoLopHocPhans.AddRange(dsMoLop);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Mở lớp học phần thành công";
                return View();
            }



        }

        //HIỂN THỊ DANH SÁCH LỚP HỌC PHÂN ĐANG MỞ
        public async Task<IActionResult> DsLopDangMo()
        {
            var dsMoLopHp = await _context.MoLopHocPhans
                .Include(m => m.MaHpNavigation)
                .ToListAsync();
            ViewBag.ngayDong = dsMoLopHp.FirstOrDefault()?.NgayDong;

            if (dsMoLopHp == null || !dsMoLopHp.Any())
            {
                TempData["Error"] = "Chưa có lớp học phần nào được mở";
                return View();
            }
            return View(dsMoLopHp);

        }

        //CHỈNH SỬA SỐ LƯỢNG
        [HttpPost]
        public async Task<IActionResult> CapNhatSL(int MaMoLop, int SoLuong)
        {
            var moLopHp = await _context.MoLopHocPhans.FindAsync(MaMoLop);
            if (moLopHp == null)
            {
                return NotFound();
            }

            moLopHp.SoLuong = SoLuong;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật số lượng thành công";
            return RedirectToAction("DsLopDangMo");
        }
    }
}