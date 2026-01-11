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
    public class QuanLyLHPController : Controller
    {
        private readonly QuanLyDiemContext _context;

        public QuanLyLHPController(QuanLyDiemContext context)
        {
            _context = context;
        }

        // GET: QuanLyLHP
        public async Task<IActionResult> Index()
        {
            var quanLyDiemContext = _context.DangKyMonHocs.Include(d => d.MaHpNavigation).Include(d => d.MaSvNavigation);
            return View(await quanLyDiemContext.ToListAsync());
        }

        // GET: QuanLyLHP/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dangKyMonHoc = await _context.DangKyMonHocs
                .Include(d => d.MaHpNavigation)
                .Include(d => d.MaSvNavigation)
                .FirstOrDefaultAsync(m => m.MaDk == id);
            if (dangKyMonHoc == null)
            {
                return NotFound();
            }

            return View(dangKyMonHoc);
        }

        // GET: QuanLyLHP/Create
        public IActionResult Create()
        {
            ViewData["MaHp"] = new SelectList(_context.HocPhans, "MaHp", "MaHp");
            ViewData["MaSv"] = new SelectList(_context.SinhViens, "MaSv", "MaSv");
            return View();
        }

        // POST: QuanLyLHP/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDk,MaSv,MaHp,HocKy,NamHoc,NgayDangKy")] DangKyMonHoc dangKyMonHoc)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dangKyMonHoc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaHp"] = new SelectList(_context.HocPhans, "MaHp", "MaHp", dangKyMonHoc.MaHp);
            ViewData["MaSv"] = new SelectList(_context.SinhViens, "MaSv", "MaSv", dangKyMonHoc.MaSv);
            return View(dangKyMonHoc);
        }

        // GET: QuanLyLHP/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dangKyMonHoc = await _context.DangKyMonHocs.FindAsync(id);
            if (dangKyMonHoc == null)
            {
                return NotFound();
            }
            ViewData["MaHp"] = new SelectList(_context.HocPhans, "MaHp", "MaHp", dangKyMonHoc.MaHp);
            ViewData["MaSv"] = new SelectList(_context.SinhViens, "MaSv", "MaSv", dangKyMonHoc.MaSv);
            return View(dangKyMonHoc);
        }

        // POST: QuanLyLHP/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaDk,MaSv,MaHp,HocKy,NamHoc,NgayDangKy")] DangKyMonHoc dangKyMonHoc)
        {
            if (id != dangKyMonHoc.MaDk)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dangKyMonHoc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DangKyMonHocExists(dangKyMonHoc.MaDk))
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
            ViewData["MaHp"] = new SelectList(_context.HocPhans, "MaHp", "MaHp", dangKyMonHoc.MaHp);
            ViewData["MaSv"] = new SelectList(_context.SinhViens, "MaSv", "MaSv", dangKyMonHoc.MaSv);
            return View(dangKyMonHoc);
        }

        // GET: QuanLyLHP/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dangKyMonHoc = await _context.DangKyMonHocs
                .Include(d => d.MaHpNavigation)
                .Include(d => d.MaSvNavigation)
                .FirstOrDefaultAsync(m => m.MaDk == id);
            if (dangKyMonHoc == null)
            {
                return NotFound();
            }

            return View(dangKyMonHoc);
        }

        // POST: QuanLyLHP/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dangKyMonHoc = await _context.DangKyMonHocs.FindAsync(id);
            if (dangKyMonHoc != null)
            {
                _context.DangKyMonHocs.Remove(dangKyMonHoc);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DangKyMonHocExists(int id)
        {
            return _context.DangKyMonHocs.Any(e => e.MaDk == id);
        }
    }
}
