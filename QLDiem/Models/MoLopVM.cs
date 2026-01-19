using Microsoft.AspNetCore.Mvc.Rendering;

namespace QLDiem.Models
{
    public class MoLopVM

    {
        public int HocKy { get; set; }
        public string NamHoc { get; set; }
        public int SoLuong  { get; set; }
        public DateTime? NgayMo { get; set; }
        public DateTime? NgayDong { get; set; }

    }
}
