using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using QLDiem.Models;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Spreadsheet;

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



    
        //GET SỬA ĐIỂM
        public async Task<IActionResult> SuaDiem(int? id)
        {

            var diem = _context.Diems
                .Include(d => d.MaLopHpNavigation)
                .Include(d => d.MaSvNavigation)
                .FirstOrDefault(m => m.MaDiem == id);
            ViewBag.DiemModel = diem;
            ViewBag.ShowSuaDiem = true;

            return View(diem);

        }
        //Xóa ĐIỂM
       
        public async Task<IActionResult> XoaDiem(int? id)
        {  
            var diem = await _context.Diems.FindAsync(id);
         
            try
            {
                _context.Diems.Remove(diem);
                await _context.SaveChangesAsync();
                return RedirectToAction("FillLopHP", new { MaLopHP = diem.MaLopHp });
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Lỗi khi xóa điểm: " + ex.Message);
                return RedirectToAction("FillLopHP", new { MaLopHP = diem.MaLopHp });
            }
        }
        //SỬA ĐIỂM
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaDiem(Diem diem)
        {
            if (diem == null) return NotFound();
            var diemCu = await _context.Diems.FindAsync(diem.MaDiem);
            if (diemCu == null) return NotFound();

            var lopHocPhan = await _context.LopHocPhans
                                .Include(l => l.MaHpNavigation)
                                .FirstOrDefaultAsync(l => l.MaLopHp == diem.MaLopHp);
            double heSo = lopHocPhan?.MaHpNavigation?.HeSo ?? 1.0;

            diemCu.DiemQt = diem.DiemQt;
            diemCu.DiemCk = diem.DiemCk;
            diemCu.DiemTk = tinhDiemTK(heSo, diem.DiemQt.Value, diem.DiemCk.Value);
            try
            {
                _context.Update(diemCu);
                await _context.SaveChangesAsync();
                return RedirectToAction("FillLopHP", new { MaLopHP = diem.MaLopHp });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật điểm: " + ex.Message);
                return RedirectToAction("FillLopHP", new { MaLopHP = diem.MaLopHp });
            }
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
                                KetQua = diem != null && diem.DiemTk >= 4.0,
                                MaLopHpNavigation = lhp
                            }).ToListAsync();

            //Xử lý thông kê sinh viên đạt và rớt
            ViewBag.SiSo = ds.Count;
            ViewBag.SvRot = ds.Count(d => d.KetQua == false);
            ViewBag.SvDat = ds.Count(d => d.KetQua == true);

            int soSvDat = ds.Count(d => d.KetQua == true);
            int soSvRot = ds.Count(d => d.KetQua == false);

            double phanTramDat = phanTramSvDat(soSvDat, ds.Count);
            double phanTramRot = phanTramSvRot(soSvRot, ds.Count);
            ViewBag.PhanTramDat = phanTramDat;
            ViewBag.PhanTramRot = phanTramRot;

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

                double heSo = 1.0; 

       
                if (lopHocPhan?.MaHpNavigation?.HeSo != null)
                {
                    heSo = lopHocPhan.MaHpNavigation.HeSo.Value;
                }

                d.DiemTk = tinhDiemTK(heSo, d.DiemQt.Value, d.DiemCk.Value);
                _context.Add(d);
                await _context.SaveChangesAsync();
                HttpContext.Session.SetString("LuuMaHp", d.MaLopHp);

                await CapNhatGpa(d.MaDiem, lopHocPhan.HocKy, lopHocPhan.NamHoc);
                return RedirectToAction("FillLopHP", new { MaLopHP = d.MaLopHp });

                

            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Lỗi khi lưu điểm: " + ex.Message);
                return RedirectToAction("FillLopHP", new { MaLopHP = d.MaLopHp });
            }

        }
        //HÀM CẬP NHẬT ĐIỂM GPA
        public async Task CapNhatGpa(int maDiem, int hocKy, string namHoc)
        {
            var diem = await _context.Diems.Include(d=>d.MaLopHpNavigation).ThenInclude(l=> l.MaHpNavigation)
                        .FirstOrDefaultAsync(d => d.MaDiem == maDiem);

            if (diem == null) return;


            var maSV = diem.MaSv;
            //++++Tính điểm Gpa học kỳ
            var diemHocKy = await _context.Diems
               .Include(d => d.MaLopHpNavigation)
                   .ThenInclude(l => l.MaHpNavigation)
               .Where(d => d.MaSv == maSV &&
                           d.MaLopHpNavigation.HocKy == hocKy &&
                           d.MaLopHpNavigation.NamHoc == namHoc &&
                           d.DiemTk.HasValue)
               .ToListAsync();

            double tongDiemHK = 0;
            int soTChocKy = 0;

            foreach (var item in diemHocKy)
            {
                var soTC = item.MaLopHpNavigation.MaHpNavigation.SoTinChi;
                tongDiemHK += (double)(item.DiemTk.Value * soTC);
                soTChocKy += (int)soTC;

            }
            double gpaHocKy = soTChocKy > 0 ? tongDiemHK / soTChocKy : 0;

            //++++Tính điểm Gpa tích lũy
            var tatCaDiem = await _context.Diems
                .Include(d => d.MaLopHpNavigation)
                    .ThenInclude(l => l.MaHpNavigation)
                .Where(d => d.MaSv == maSV && d.DiemTk.HasValue)
                .ToListAsync();

            double tongDiemTL = 0;
            int soTCTichLuy = 0;

            foreach (var item in tatCaDiem)
            {
                var soTC = item.MaLopHpNavigation.MaHpNavigation.SoTinChi;
                tongDiemTL += (double)(item.DiemTk.Value * soTC);
                soTCTichLuy += (int)soTC;
            }

            double gpaTichLuy = soTCTichLuy > 0 ? tongDiemTL / soTCTichLuy : 0;

            // Cập nhật hoặc thêm mới bảng Gpa
            var gpaCheck = await _context.Gpas.FirstOrDefaultAsync(g => g.MaSv == maSV && g.HocKy == hocKy && g.NamHoc == namHoc);
            if (gpaCheck == null)
            {
                
                gpaCheck  = new Gpa
                {
                
                    MaSv = maSV,
                    HocKy = hocKy,
                    NamHoc = namHoc,
                     GpaHocKy = gpaHocKy,
                     GpaTichLuy = gpaTichLuy,
                    SoTcHocKy = soTChocKy,
                   SoTcTichLuy = soTCTichLuy
                };
                _context.Gpas.Add(gpaCheck);
            }
            else
            {
                gpaCheck.GpaHocKy = gpaHocKy;
                gpaCheck.GpaTichLuy = gpaTichLuy;
                gpaCheck.SoTcHocKy = soTChocKy;
                gpaCheck.SoTcTichLuy = soTCTichLuy;
            }
            await _context.SaveChangesAsync();
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


        ///Tính % số sinh viên đạt và rớt trong lớp học
        public double phanTramSvDat(int soSvDat, int tongSoSv)
        {
            if (tongSoSv == 0) return 0;
            return Math.Round(((double)soSvDat / tongSoSv) * 100, 2);
        }

        public double phanTramSvRot(int soSvRot, int tongSoSv)
        {
            if (tongSoSv == 0) return 0;
            return Math.Round(((double)soSvRot / tongSoSv) * 100, 2);
        }



        //THỐNG KÊ
        //IN BẢNG ĐIỂM CỦA LỚP HỌC PHẦN
        [HttpGet]
        public async Task<IActionResult> InBangDiem(string id)
        {
            if (id == null)
            {
                TempData["Error"] = "Vui lòng chọn lớp học phần";
                return RedirectToAction("Index");
            }

            var diem = await _context.Diems
                .Include(d => d.MaLopHpNavigation)
                .Include(d => d.MaSvNavigation)
                .Where(d => d.MaLopHp == id.ToString())
                .ToListAsync();
            if (diem == null || !diem.Any())
            {
                TempData["Error"] = "Lớp chưa có sinh viên nào có điểm";
                return RedirectToAction("Index");
            }

            var lopHocPhan = await _context.LopHocPhans
                              .Include(l => l.MaHpNavigation)
                              .FirstOrDefaultAsync(l => l.MaLopHp == id);
            string tenLopHp = lopHocPhan.MaHpNavigation.TenHp.ToString();

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Danh sách ");

            // Thêm tiêu đề cột
            worksheet.Cell(1, 1).Value = lopHocPhan.MaLopHp+"-"+tenLopHp + "-HK-" + lopHocPhan.HocKy +"-Năm-"+ lopHocPhan.NamHoc;
            worksheet.Cell(2, 1).Value = "Mã Sinh Viên";
            worksheet.Cell(2, 2).Value = "Họ Tên";
            worksheet.Cell(2, 3).Value = "Lớp";
            worksheet.Cell(2, 4).Value = "Điểm Quá Trình";
            worksheet.Cell(2, 5).Value = "Điểm Cuối Kỳ";
            worksheet.Cell(2, 6).Value = "Điểm Tổng Kết";
            worksheet.Cell(2, 7).Value = "Kết Quả";

            // Lấy danh sách điểm
            int row = 3;
            foreach (var item in diem)
            {
                worksheet.Cell(row, 1).Value = item.MaSv;
                worksheet.Cell(row, 2).Value = item.MaSvNavigation?.HoTen;
                worksheet.Cell(row, 3).Value = item.MaSvNavigation?.Lop;
                worksheet.Cell(row, 4).Value = item.DiemQt;
                worksheet.Cell(row, 5).Value = item.DiemCk;
                worksheet.Cell(row, 6).Value = item.DiemTk;
                worksheet.Cell(row, 7).Value = item.DiemTk >= 4.0 ? "Đạt" : "Rớt";

                row++;
            }

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BangDiem.xlsx");
            }
        }

        //IN BẢNG ĐIỂM DANH SÁCH SINH VIÊN RỚT MÔN
        [HttpGet]
        public async Task<IActionResult> InDsSvRot(string id)
        {
            if (id == null)
            {
                TempData["Error"] = "Vui lòng chọn lớp học phần";
                return RedirectToAction("Index");
            }
            var diem = await _context.Diems
                .Include(d => d.MaLopHpNavigation)
                .Include(d => d.MaSvNavigation)
                .Where(d => d.MaLopHp == id.ToString() && d.DiemTk < 4.0)
                .ToListAsync();
            if (diem == null || !diem.Any())
            {
                TempData["Error"] = "Lớp chưa có sinh viên nào rớt môn";
                return RedirectToAction("Index");
            }

            var lopHocPhan = await _context.LopHocPhans
                              .Include(l => l.MaHpNavigation)
                              .FirstOrDefaultAsync(l => l.MaLopHp == id);
            string tenLopHp = lopHocPhan.MaHpNavigation.TenHp.ToString();

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Danh sách ");

            // Thêm tiêu đề cột
            worksheet.Cell(1, 1).Value = lopHocPhan.MaLopHp + "-" + tenLopHp + "-HK-" + lopHocPhan.HocKy + "-Năm-" + lopHocPhan.NamHoc;
            worksheet.Cell(2, 1).Value = "Mã Sinh Viên";
            worksheet.Cell(2, 2).Value = "Họ Tên";
            worksheet.Cell(2, 3).Value = "Lớp";
            worksheet.Cell(2, 4).Value = "Điểm Quá Trình";
            worksheet.Cell(2, 5).Value = "Điểm Cuối Kỳ";
            worksheet.Cell(2, 6).Value = "Điểm Tổng Kết";
            worksheet.Cell(2, 7).Value = "Kết Quả";

            // Lấy danh sách điểm
            int row = 3;
            foreach (var item in diem)
            {
                worksheet.Cell(row, 1).Value = item.MaSv;
                worksheet.Cell(row, 2).Value = item.MaSvNavigation?.HoTen;
                worksheet.Cell(row, 3).Value = item.MaSvNavigation?.Lop;
                worksheet.Cell(row, 4).Value = item.DiemQt;
                worksheet.Cell(row, 5).Value = item.DiemCk;
                worksheet.Cell(row, 6).Value = item.DiemTk;
                worksheet.Cell(row, 7).Value = "Rớt";

                row++;
            }

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BangDiemSVR.xlsx");
            }
        }


        //IN BẢNG ĐIỂM DANH SÁCH SINH VIÊN QUA MÔN
        [HttpGet]
        public async Task<IActionResult> InDsSvDat(string id)
        {
            if (id == null)
            {
                TempData["Error"] = "Vui lòng chọn lớp học phần";
                return RedirectToAction("Index");
            }
            var diem = await _context.Diems
                .Include(d => d.MaLopHpNavigation)
                .Include(d => d.MaSvNavigation)
                .Where(d => d.MaLopHp == id.ToString() && d.DiemTk >= 4.0)
                .ToListAsync();
            if (diem == null || !diem.Any())
            {
                TempData["Error"] = "Lớp chưa có sinh viên nào qua môn";
                return RedirectToAction("Index");
            }

            var lopHocPhan = await _context.LopHocPhans
                              .Include(l => l.MaHpNavigation)
                              .FirstOrDefaultAsync(l => l.MaLopHp == id);
            string tenLopHp = lopHocPhan.MaHpNavigation.TenHp.ToString();

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Danh sách ");

            // Thêm tiêu đề cột
            worksheet.Cell(1, 1).Value = lopHocPhan.MaLopHp + "-" + tenLopHp + "-HK-" + lopHocPhan.HocKy + "-Năm-" + lopHocPhan.NamHoc;

            worksheet.Cell(2, 1).Value = "Mã Sinh Viên";
            worksheet.Cell(2, 2).Value = "Họ Tên";
            worksheet.Cell(2, 3).Value = "Lớp";
            worksheet.Cell(2, 4).Value = "Điểm Quá Trình";
            worksheet.Cell(2, 5).Value = "Điểm Cuối Kỳ";
            worksheet.Cell(2, 6).Value = "Điểm Tổng Kết";
            worksheet.Cell(2, 7).Value = "Kết Quả";

            // Lấy danh sách điểm
            int row = 3;
            foreach (var item in diem)
            {
                worksheet.Cell(row, 1).Value = item.MaSv;
                worksheet.Cell(row, 2).Value = item.MaSvNavigation?.HoTen;
                worksheet.Cell(row, 3).Value = item.MaSvNavigation?.Lop;
                worksheet.Cell(row, 4).Value = item.DiemQt;
                worksheet.Cell(row, 5).Value = item.DiemCk;
                worksheet.Cell(row, 6).Value = item.DiemTk;
                worksheet.Cell(row, 7).Value = "Đạt";

                row++;
            }

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BangDiemSVĐ.xlsx");
            }
        }


    }

       
    }
