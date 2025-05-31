using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;

namespace MyWebApp.Models
{
    public class CheckoutViewModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    public string FullName { get; set; }
    
    [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
    public string Address { get; set; }
    
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    public string PhoneNumber { get; set; }
    
    [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
    public PaymentMethod PaymentMethod { get; set; } 
    
    public List<ItemCart> Items { get; set; } = new List<ItemCart>();
    
    // Các trường mới thêm
    public decimal SubTotal { get; set; }
    public decimal ShippingFee { get; set; } = 30000m;
    public decimal DesignFee { get; set; } = 50000m;
    public decimal TotalAmount { get; set; }

    [BindNever]
    public List<HoaDonChiTiet> ChiTietHoaDon { get; set; } = new List<HoaDonChiTiet>();

    // Phương thức tính toán
    public void CalculateTotals()
    {
        SubTotal = Items?.Sum(item => item.Product?.Price * item.Quantity) ?? 0m;
        TotalAmount = SubTotal + ShippingFee + DesignFee;
    }
}
}