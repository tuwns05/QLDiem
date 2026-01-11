using System;
using System.Collections.Generic;

namespace QLDiem.Models;

public partial class SinhVien
{
    public string MaSv { get; set; } = null!;

    public string? HoTen { get; set; }

    public string? Lop { get; set; }

    public string? GioiTinh { get; set; }

    public DateOnly? NgaySinh { get; set; }

    public virtual ICollection<DangKyMonHoc> DangKyMonHocs { get; set; } = new List<DangKyMonHoc>();

    public virtual ICollection<DiemRenLuyen> DiemRenLuyens { get; set; } = new List<DiemRenLuyen>();

    public virtual ICollection<Diem> Diems { get; set; } = new List<Diem>();

    public virtual ICollection<Gpa> Gpas { get; set; } = new List<Gpa>();

    public virtual ICollection<TaiKhoan> TaiKhoans { get; set; } = new List<TaiKhoan>();
}
