namespace QLDiem.Models
{
    public class DiemSinhVienVm
    {
        public string MaSv { get; set; }

        public string MaHp { get; set; }
        public string TenHp { get; set; }
        public int SoTinChi { get; set; }

        public int HocKy { get; set; }
        public string NamHoc { get; set; }

        public double? DiemQt { get; set; }
        public double? DiemCk { get; set; }
        public double? DiemTk { get; set; }

        public bool KetQua { get; set; }
    }
}
