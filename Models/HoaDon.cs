using System.ComponentModel.DataAnnotations;
using MyWebApp.Models;

public class HoaDon
{
    public int Id { get; set; }

    
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }

    public DateTime NgayLap { get; set; } = DateTime.Now;

    
    public string TenNguoiNhan { get; set; }

    
    public string DiaChi { get; set; }

    
    public string SoDienThoai { get; set; }

    public decimal TongTien { get; set; }

    // Thêm 2 thuộc tính mới:
    public PaymentMethod PaymentMethod { get; set; } // COD, BankTransfer, MoMo
    public string Status { get; set; } // Chờ xác nhận, Chờ thanh toán, Đã thanh toán, v.v.

    public ICollection<HoaDonChiTiet> ChiTietHoaDon { get; set; }
}
