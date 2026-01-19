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
    public class QLSinhVienController : Controller
    {
        private readonly QuanLyDiemContext _context;

        public QLSinhVienController(QuanLyDiemContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? lop,string? maSv)
        {
            var query = _context.SinhViens.AsQueryable();

            if (!string.IsNullOrEmpty(maSv))
            {
                query = query.Where(sv => sv.MaSv.Contains(maSv));
            }

            if (!string.IsNullOrEmpty(lop))
            {
                query = query.Where(sv => sv.Lop.Contains(lop));
            }

            ViewBag.DanhSachLop = await _context.SinhViens
                .Select(sv => sv.Lop)
                .Distinct()
                .OrderBy(l => l)
                .ToListAsync();

            ViewBag.Lop = lop;
            ViewBag.MaSv = maSv;
            return View(await query.ToListAsync());
        }


        // GET: QLSinhVien/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var sinhVien = await _context.SinhViens
                .FirstOrDefaultAsync(sv => sv.MaSv == id);

            if (sinhVien == null) return NotFound();

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
                    SoTinChi = hp.SoTinChi,

                    HocKy = lhp.HocKy,
                    NamHoc = lhp.NamHoc,

                    DiemQt = diem != null ? diem.DiemQt : null,
                    DiemCk = diem != null ? diem.DiemCk : null,
                    DiemTk = diem != null ? diem.DiemTk : null,

                    KetQua = diem != null && diem.DiemTk >= 4
                }
            ).ToListAsync();

            ViewBag.DiemSV = diemSV;

            return View(sinhVien);
        }

        // GET: QLSinhVien/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: QLSinhVien/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaSv,HoTen,Lop,GioiTinh,NgaySinh")] SinhVien sinhVien)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sinhVien);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sinhVien);
        }

        // GET: QLSinhVien/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sinhVien = await _context.SinhViens.FindAsync(id);
            if (sinhVien == null)
            {
                return NotFound();
            }
            return View(sinhVien);
        }

        // POST: QLSinhVien/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaSv,HoTen,Lop,GioiTinh,NgaySinh")] SinhVien sinhVien)
        {
            if (id != sinhVien.MaSv)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sinhVien);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SinhVienExists(sinhVien.MaSv))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(sinhVien);
        }

        // GET: QLSinhVien/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var sinhVien = await _context.SinhViens
                .FirstOrDefaultAsync(m => m.MaSv == id);

            if (sinhVien == null)
            {
                return NotFound();
            }

            return View(sinhVien);
        }

        // POST: QLSinhVien/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var sinhVien = await _context.SinhViens
                .FirstOrDefaultAsync(m => m.MaSv == id);

            if (sinhVien != null)
            {
                _context.SinhViens.Remove(sinhVien);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }



        private bool SinhVienExists(string id)
        {
            return _context.SinhViens.Any(e => e.MaSv == id);
        }
    }
}
