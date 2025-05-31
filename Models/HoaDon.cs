using System.ComponentModel.DataAnnotations;
using MyWebApp.Models;

public class HoaDon
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string TenNguoiNhan { get; set; }
    public string DiaChi { get; set; }
    public string SoDienThoai { get; set; }
    
    // Các trường mới
    public decimal SubTotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal DesignFee { get; set; }
    public decimal TongTien { get; set; }
    
    public PaymentMethod PaymentMethod { get; set; }
    public string Status { get; set; }
    public DateTime NgayLap { get; set; }
    
    public virtual ICollection<HoaDonChiTiet> ChiTietHoaDon { get; set; } = new List<HoaDonChiTiet>();
}
