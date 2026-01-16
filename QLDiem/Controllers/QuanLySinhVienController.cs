//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using QLDiem.Models;

//namespace QLDiem.Controllers
//{
//    public class QuanLySinhVienController : Controller
//    {
//        private readonly QuanLyDiemContext _context;

//        public QuanLySinhVienController(QuanLyDiemContext context)
//        {
//            _context = context;
//        }
//        // =========================
//        // DANH SÁCH SINH VIÊN + LỌC THEO LỚP HỌC PHẦN
//        // =========================
//        public async Task<IActionResult> Index(string? maHP, string? lop)
//        {
//            // ComboBox Mã học phần
//            ViewBag.HocPhans = new SelectList(
//                await _context.HocPhans.ToListAsync(),
//                "MaHp",
//                "MaHp",
//                maHP
//            );

//            // Query gốc
//            var query = _context.SinhViens.AsQueryable();

//            // Lọc theo Mã học phần
//            if (!string.IsNullOrEmpty(maHP))
//            {
//                query = from sv in query
//                        join d in _context.Diems on sv.MaSv equals d.MaSv
//                        where d.MaHp == maHP
//                        select sv;
//            }

//            // Lọc theo Lớp sinh hoạt
//            if (!string.IsNullOrEmpty(lop))
//            {
//                query = query.Where(sv => sv.Lop.Contains(lop));
//            }

//            ViewBag.lop = lop;

//            return View(await query.Distinct().ToListAsync());
//        }

//        // GET: QuanLySinhVien/Details/5
//        public async Task<IActionResult> Details(string id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var sinhVien = await _context.SinhViens
//                .FirstOrDefaultAsync(m => m.MaSv == id);
//            if (sinhVien == null)
//            {
//                return NotFound();
//            }
//            var bangDiem = await (
//                    from d in _context.Diems
//                    join hp in _context.HocPhans on d.MaHp equals hp.MaHp
//                    where d.MaSv == id
//                    select new
//                    {
//                        TenHP = hp.TenHp,
//                        d.HocKy,
//                        d.NamHoc,
//                        d.DiemQt,
//                        d.DiemCk,
//                        d.DiemTk
//                    }
//                ).ToListAsync();

//            ViewBag.BangDiem = bangDiem;

//            return View(sinhVien);
//        }

//        // GET: QuanLySinhVien/Create
//        public IActionResult Create()
//        {
//            return View();
//        }

//        // POST: QuanLySinhVien/Create
//        // To protect from overposting attacks, enable the specific properties you want to bind to.
//        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create([Bind("MaSv,HoTen,Lop,GioiTinh,NgaySinh")] SinhVien sinhVien)
//        {
//            if (ModelState.IsValid)
//            {
//                // 1. Thêm sinh viên
//                _context.SinhViens.Add(sinhVien);

//                // 2. Tạo tài khoản cho sinh viên
//                var taiKhoan = new TaiKhoan
//                {
//                    TenDangNhap = sinhVien.MaSv,
//                    MatKhau = sinhVien.MaSv,   // mật khẩu mặc định
//                    VaiTro = "SinhVien",
//                    MaSv = sinhVien.MaSv
//                };

//                _context.TaiKhoans.Add(taiKhoan);

//                // 3. Lưu 1 lần
//                await _context.SaveChangesAsync();

//                return RedirectToAction(nameof(Index));
//            }
//            return View(sinhVien);
//        }

//        // GET: QuanLySinhVien/Edit/5
//        public async Task<IActionResult> Edit(string id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var sinhVien = await _context.SinhViens.FindAsync(id);
//            if (sinhVien == null)
//            {
//                return NotFound();
//            }
//            return View(sinhVien);
//        }

//        // POST: QuanLySinhVien/Edit/5
//        // To protect from overposting attacks, enable the specific properties you want to bind to.
//        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(string id, [Bind("MaSv,HoTen,Lop,GioiTinh,NgaySinh")] SinhVien sinhVien)
//        {
//            if (id != sinhVien.MaSv)
//            {
//                return NotFound();
//            }

//            if (ModelState.IsValid)
//            {
//                try
//                {
//                    _context.Update(sinhVien);
//                    await _context.SaveChangesAsync();
//                }
//                catch (DbUpdateConcurrencyException)
//                {
//                    if (!SinhVienExists(sinhVien.MaSv))
//                    {
//                        return NotFound();
//                    }
//                    else
//                    {
//                        throw;
//                    }
//                }
//                return RedirectToAction(nameof(Index));
//            }
//            return View(sinhVien);
//        }

//        // GET: QuanLySinhVien/Delete/5
//        public async Task<IActionResult> Delete(string id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var sinhVien = await _context.SinhViens
//                .FirstOrDefaultAsync(m => m.MaSv == id);
//            if (sinhVien == null)
//            {
//                return NotFound();
//            }

//            return View(sinhVien);
//        }

//        // POST: QuanLySinhVien/Delete/5
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(string id)
//        {
//            var sinhVien = await _context.SinhViens.FindAsync(id);
//            if (sinhVien != null)
//            {
//                _context.SinhViens.Remove(sinhVien);
//            }

//            await _context.SaveChangesAsync();
//            return RedirectToAction(nameof(Index));
//        }

//        private bool SinhVienExists(string id)
//        {
//            return _context.SinhViens.Any(e => e.MaSv == id);
//        }

//    }
//}
