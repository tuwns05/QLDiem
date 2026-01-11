using System;
using System.Collections.Generic;

namespace QLDiem.Models;

public partial class DiemRenLuyen
{
    public int MaDrl { get; set; }

    public string? MaSv { get; set; }

    public int? HocKy { get; set; }

    public string? NamHoc { get; set; }

    public int? DiemRl { get; set; }

    public virtual SinhVien? MaSvNavigation { get; set; }
}
