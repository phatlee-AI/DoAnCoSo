using System.ComponentModel.DataAnnotations;

namespace MyWebApp.Models
{
    public class DesignRequest
    {
        public int Id { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        
        // Thông tin màu sắc
        public string FrontColor { get; set; } = "#FFFFFF";
        public string BackColor { get; set; } = "#FFFFFF";
        public string CollarColor { get; set; } = "#000000";
        
        // Thông tin hình ảnh
        public string? ImagePath { get; set; }
        public int ImagePositionX { get; set; } = 25;
        public int ImagePositionY { get; set; } = 20;
        public int ImageSize { get; set; } = 100;
        public int ImageRotate { get; set; } = 0;
        
        // Thông tin text
        public string? TextContent { get; set; }
        public string TextFont { get; set; } = "Arial";
        public string TextColor { get; set; } = "#000000";
        public int TextSize { get; set; } = 20;
        public int TextPositionY { get; set; } = 15;
        
        // Thông tin khách hàng
        [Required]
        public string CustomerName { get; set; } = string.Empty;
        
        [Required, EmailAddress]
        public string CustomerEmail { get; set; } = string.Empty;
        
        [Required, Phone]
        public string CustomerPhone { get; set; } = string.Empty;
        
        public string? CustomerNote { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Chờ xử lý"; // Chờ xử lý, Đang thiết kế, Hoàn thành
    }
}