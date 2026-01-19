using System;
using System.Collections.Generic;

namespace QLDiem.Models;

public partial class HocPhan
{
    public string MaHp { get; set; } = null!;

    public string? TenHp { get; set; }

    public int? SoTinChi { get; set; }

    public double? HeSo { get; set; }

    public virtual ICollection<LopHocPhan> LopHocPhans { get; set; } = new List<LopHocPhan>();

    public virtual ICollection<MoLopHocPhan> MoLopHocPhans { get; set; } = new List<MoLopHocPhan>();
}
