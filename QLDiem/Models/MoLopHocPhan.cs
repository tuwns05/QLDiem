using System;
using System.Collections.Generic;

namespace QLDiem.Models;

public partial class MoLopHocPhan
{
    public int MaMoLop { get; set; }

    public string MaHp { get; set; } = null!;

    public int HocKy { get; set; }

    public string NamHoc { get; set; } = null!;

    public int SoLuong { get; set; }

    public int SoLuongHienTai { get; set; }

    public DateTime? NgayMo { get; set; }

    public DateTime? NgayDong { get; set; }

    public virtual HocPhan MaHpNavigation { get; set; } = null!;
}
