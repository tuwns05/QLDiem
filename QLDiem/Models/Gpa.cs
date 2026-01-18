using System;
using System.Collections.Generic;

namespace QLDiem.Models;

public partial class Gpa
{
    public int MaGpa { get; set; }

    public string? MaSv { get; set; }

    public int? HocKy { get; set; }

    public string? NamHoc { get; set; }

    public double? GpaTichLuy { get; set; }

    public double? GpaHocKy { get; set; }

    public int? SoTcHocKy { get; set; }

    public int? SoTcTichLuy { get; set; }

    public virtual SinhVien? MaSvNavigation { get; set; }
}
