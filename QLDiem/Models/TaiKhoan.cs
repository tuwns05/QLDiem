using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDiem.Models;

public partial class TaiKhoan
{
    public int MaTk { get; set; }

    public string TenDangNhap { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string? VaiTro { get; set; }

    public string? MaSv { get; set; }

    [NotMapped]
    public string CurrentPassword { get; set; }

    [NotMapped]
    public string NewPassword { get; set; }

    [NotMapped]
    public string ConfirmPassword { get; set; }
}
