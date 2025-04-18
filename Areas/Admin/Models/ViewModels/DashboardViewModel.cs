using System;
using System.Collections.Generic;

namespace MyWebApp.Areas.Admin.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }

        public List<string> RevenueMonths { get; set; }
        public List<decimal> MonthlyRevenue { get; set; }

        public List<string> RevenueDates { get; set; }       // Danh sách ngày (dạng "dd/MM")
        public List<decimal> DailyRevenue { get; set; }      // Tổng tiền theo từng ngày


        public List<DonHangMoiNhatVM> LatestOrders { get; set; }
        public List<TopProductVM> TopSellingProducts { get; set; }
    }

    public class DonHangMoiNhatVM
    {
        public int Id { get; set; }
        public string TenNguoiNhan { get; set; }
        public decimal TongTien { get; set; }
        public DateTime NgayLap { get; set; }
        public string Status { get; set; }
    }

    public class TopProductVM
    {
        public string ProductName { get; set; }
        public int SoLuongBan { get; set; }
        public string ImageUrl { get; set; } 
        public string Categories { get; set; } 
    }
}
