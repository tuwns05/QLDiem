using System;
using System.Collections.Generic;

namespace QLDiem.Models;

public partial class DangKyMonHoc
{
    public int MaDk { get; set; }

    public string MaSv { get; set; } = null!;

    public string MaHp { get; set; } = null!;

    public int HocKy { get; set; }

    public string NamHoc { get; set; } = null!;

    public DateTime? NgayDangKy { get; set; }

    public virtual HocPhan MaHpNavigation { get; set; } = null!;

    public virtual SinhVien MaSvNavigation { get; set; } = null!;
}
