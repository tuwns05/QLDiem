using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> Index()
        {
            var quanLyDiemContext = _context.Diems.Include(d => d.MaHpNavigation).Include(d => d.MaSvNavigation);
            return View(await quanLyDiemContext.ToListAsync());
        }

        // GET: Admin/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var diem = await _context.Diems
                .Include(d => d.MaHpNavigation)
                .Include(d => d.MaSvNavigation)
                .FirstOrDefaultAsync(m => m.MaDiem == id);
            if (diem == null)
            {
                return NotFound();
            }

            return View(diem);
        }

        // GET: Admin/Create
        public IActionResult Create()
        {
            ViewData["MaHp"] = new SelectList(_context.HocPhans, "MaHp", "MaHp");
            ViewData["MaSv"] = new SelectList(_context.SinhViens, "MaSv", "MaSv");
            return View();
        }

        // POST: Admin/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDiem,MaSv,MaHp,HocKy,NamHoc,DiemQt,DiemCk,DiemTk")] Diem diem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(diem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaHp"] = new SelectList(_context.HocPhans, "MaHp", "MaHp", diem.MaHp);
            ViewData["MaSv"] = new SelectList(_context.SinhViens, "MaSv", "MaSv", diem.MaSv);
            return View(diem);
        }

        // GET: Admin/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var diem = await _context.Diems.FindAsync(id);
            if (diem == null)
            {
                return NotFound();
            }
            ViewData["MaHp"] = new SelectList(_context.HocPhans, "MaHp", "MaHp", diem.MaHp);
            ViewData["MaSv"] = new SelectList(_context.SinhViens, "MaSv", "MaSv", diem.MaSv);
            return View(diem);
        }

        // POST: Admin/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaDiem,MaSv,MaHp,HocKy,NamHoc,DiemQt,DiemCk,DiemTk")] Diem diem)
        {
            if (id != diem.MaDiem)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(diem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DiemExists(diem.MaDiem))
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
            ViewData["MaHp"] = new SelectList(_context.HocPhans, "MaHp", "MaHp", diem.MaHp);
            ViewData["MaSv"] = new SelectList(_context.SinhViens, "MaSv", "MaSv", diem.MaSv);
            return View(diem);
        }

        // GET: Admin/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var diem = await _context.Diems
                .Include(d => d.MaHpNavigation)
                .Include(d => d.MaSvNavigation)
                .FirstOrDefaultAsync(m => m.MaDiem == id);
            if (diem == null)
            {
                return NotFound();
            }

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
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DiemExists(int id)
        {
            return _context.Diems.Any(e => e.MaDiem == id);
        }
    }
}
