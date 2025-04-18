using System.ComponentModel.DataAnnotations;

namespace MyWebApp.Models
{
    public class HoaDonChiTiet
    {
        public int Id { get; set; }

        public int HoaDonId { get; set; }
        public HoaDon HoaDon { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int SoLuong { get; set; }

        public decimal DonGia { get; set; }
    }
}
