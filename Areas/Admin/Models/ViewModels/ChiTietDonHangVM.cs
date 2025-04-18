namespace MyWebApp.Areas.Admin.Models.ViewModels
{
    public class ChiTietDonHangVM
    {
        public int Id { get; set; }
        public string TenNguoiNhan { get; set; }
        public string SoDienThoai { get; set; }
        public string DiaChi { get; set; }
        public DateTime NgayLap { get; set; }
        public decimal TongTien { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }

        public List<SanPhamTrongDonHang> DanhSachSanPham { get; set; }
    }

    public class SanPhamTrongDonHang
    {
        public string TenSanPham { get; set; }
        public string HinhAnh { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien => DonGia * SoLuong;
    }
}
