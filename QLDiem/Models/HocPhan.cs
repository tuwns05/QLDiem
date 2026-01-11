using System;
using System.Collections.Generic;

namespace QLDiem.Models;

public partial class HocPhan
{
    public string MaHp { get; set; } = null!;

    public string? TenHp { get; set; }

    public int? SoTinChi { get; set; }

    public double? HeSo { get; set; }

    public virtual ICollection<DangKyMonHoc> DangKyMonHocs { get; set; } = new List<DangKyMonHoc>();

    public virtual ICollection<Diem> Diems { get; set; } = new List<Diem>();
}
