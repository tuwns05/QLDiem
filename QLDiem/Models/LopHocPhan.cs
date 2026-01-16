using System;
using System.Collections.Generic;

namespace QLDiem.Models;

public partial class LopHocPhan
{
    public string MaLopHp { get; set; } = null!;

    public string MaHp { get; set; } = null!;

    public int HocKy { get; set; }

    public string NamHoc { get; set; } = null!;

    public virtual ICollection<DangKyMonHoc> DangKyMonHocs { get; set; } = new List<DangKyMonHoc>();

    public virtual ICollection<Diem> Diems { get; set; } = new List<Diem>();

    public virtual HocPhan MaHpNavigation { get; set; } = null!;
}
