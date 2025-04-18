using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MyWebApp.Models
{
    public class CheckoutViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string FullName { get; set; }
         [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string Address { get; set; }
         [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public string PhoneNumber { get; set; }
         [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public PaymentMethod PaymentMethod { get; set; } 
        public List<ItemCart> Items { get; set; } = new List<ItemCart>();
        public decimal TotalAmount { get; set; }

        [BindNever]
        public List<HoaDonChiTiet> ChiTietHoaDon { get; set; } = new List<HoaDonChiTiet>();

       

    


        
        
    }
}
