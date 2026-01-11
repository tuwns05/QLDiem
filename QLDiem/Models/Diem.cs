using System;
using System.Collections.Generic;

namespace QLDiem.Models;

public partial class Diem
{
    public int MaDiem { get; set; }

    public string? MaSv { get; set; }

    public string? MaHp { get; set; }

    public int? HocKy { get; set; }

    public string? NamHoc { get; set; }

    public double? DiemQt { get; set; }

    public double? DiemCk { get; set; }

    public double? DiemTk { get; set; }

    public virtual HocPhan? MaHpNavigation { get; set; }

    public virtual SinhVien? MaSvNavigation { get; set; }
}
