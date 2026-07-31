using TeenNovel_Wed.Models;

namespace TeenNovel_Wed.ViewModels
{
    public class ChiTietNapViewModel
    {
        public GoiNapXu GoiNap { get; set; } = null!;

        public string QrUrl { get; set; } = string.Empty;

        public string NoiDungChuyenKhoan { get; set; } = string.Empty;

        public string SoTaiKhoan { get; set; } = string.Empty;

        public string TenTaiKhoan { get; set; } = string.Empty;

        public string TenNganHang { get; set; } = string.Empty;
    }
}