using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDiem.Models;

public partial class Diem
{
    public int MaDiem { get; set; }

    public string? MaSv { get; set; }

    public double? DiemQt { get; set; }

    public double? DiemCk { get; set; }

    public double? DiemTk { get; set; }

    public string? MaLopHp { get; set; }
   
    [NotMapped]
    public bool? KetQua { get; set; }
    public virtual LopHocPhan? MaLopHpNavigation { get; set; }

    public virtual SinhVien? MaSvNavigation { get; set; }
}
