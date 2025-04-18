using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Collections.Generic;
using MyWebApp.Models;
using MyWebApp.Areas.Admin.Models.ViewModels;

namespace MyWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Tổng số người dùng, sản phẩm, đơn hàng
            var totalUsers = _context.Users.Count();
            var totalProducts = _context.Products.Count();
            var totalOrders = _context.HoaDons.Count();

            // Tổng doanh thu (chỉ tính đơn hàng đã thanh toán)
            var totalRevenue = _context.HoaDons
                .Where(h => h.Status == "Đã thanh toán")
                .Sum(h => h.TongTien);

            // 5 đơn hàng mới nhất
            var latestOrders = _context.HoaDons
                .OrderByDescending(h => h.NgayLap)
                .Take(5)
                .Select(h => new DonHangMoiNhatVM
                {
                    Id = h.Id,
                    TenNguoiNhan = h.TenNguoiNhan,
                    TongTien = h.TongTien,
                    NgayLap = h.NgayLap,
                    Status = h.Status // hoặc h.Status nếu tên cũ
                })
                .ToList();

            // Top 5 sản phẩm bán chạy
            var topSellingProducts = _context.HoaDonChiTiets
                .GroupBy(ct => ct.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    SoLuongBan = g.Sum(ct => ct.SoLuong)
                })
                .OrderByDescending(x => x.SoLuongBan)
                .Take(5)
                .Join(_context.Products.Include(p => p.Category), // đảm bảo include Category
                    g => g.ProductId,
                    p => p.Id,
                    (g, p) => new TopProductVM
                    {
                        ProductName = p.Name,
                        SoLuongBan = g.SoLuongBan,
                        ImageUrl = p.ImageUrl, // đảm bảo Product có trường ImageUrl
                        Categories = p.Category != null ? p.Category.Name : "Không rõ"
                    })
                .ToList();

            // Doanh thu theo tháng
            var revenueByMonth = _context.HoaDons
            .Where(h => h.Status == "Đã thanh toán")
            .AsEnumerable() // Chuyển truy vấn sang client-side để dùng string.Format
            .GroupBy(h => new { h.NgayLap.Year, h.NgayLap.Month })
            .Select(g => new
            {
                Month = string.Format("{0}/{1}", g.Key.Month, g.Key.Year),
                Total = g.Sum(e => e.TongTien)
            })
            .OrderBy(x => x.Month)
            .ToList();


            var revenueMonths = revenueByMonth.Select(x => x.Month).ToList();
            var monthlyRevenue = revenueByMonth.Select(x => x.Total).ToList();

            // Gán dữ liệu vào ViewModel
            var viewModel = new DashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalProducts = totalProducts,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                LatestOrders = latestOrders,
                TopSellingProducts = topSellingProducts,
                RevenueMonths = revenueMonths,
                MonthlyRevenue = monthlyRevenue
            };

            return View(viewModel);
        }

        public IActionResult Detail(int id)
    {
        var hoaDon = _context.HoaDons
            .Include(h => h.ChiTietHoaDon)
                .ThenInclude(ct => ct.Product)
                .ThenInclude(p => p.ProductImages) // để lấy hình ảnh
            .FirstOrDefault(h => h.Id == id);

        if (hoaDon == null) return NotFound();

        var vm = new ChiTietDonHangVM
        {
            Id = hoaDon.Id,
            TenNguoiNhan = hoaDon.TenNguoiNhan,
            SoDienThoai = hoaDon.SoDienThoai,
            DiaChi = hoaDon.DiaChi,
            NgayLap = hoaDon.NgayLap,
            Status = hoaDon.Status,
            TongTien = hoaDon.TongTien,
            DanhSachSanPham = hoaDon.ChiTietHoaDon.Select(ct => new SanPhamTrongDonHang
            {
                TenSanPham = ct.Product.Name,
                SoLuong = ct.SoLuong,
                DonGia = ct.DonGia,
                HinhAnh = ct.Product.ProductImages.FirstOrDefault()?.Url // Lấy ảnh đầu tiên
            }).ToList()
        };

        return View(vm);
    }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmOrder(int id)
        {
            var order = _context.HoaDons.FirstOrDefault(h => h.Id == id);
            if (order != null && order.Status == "Chờ xác nhận")
            {
                order.Status= "Đã thanh toán";
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Orders()
        {
            var allOrders = _context.HoaDons
                .OrderByDescending(h => h.NgayLap)
                .Select(h => new DonHangMoiNhatVM
                {
                    Id = h.Id,
                    TenNguoiNhan = h.TenNguoiNhan,
                    TongTien = h.TongTien,
                    NgayLap = h.NgayLap,
                    Status = h.Status
                })
                .ToList();

            return View(allOrders);
        }



    }
}
