using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Collections.Generic;
using MyWebApp.Models;
using MyWebApp.Areas.Admin.Models.ViewModels;
using System.Security.Claims;

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
                order.Status = "Đã thanh toán";
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Report(ReportViewModel model)
        {
            if (ModelState.IsValid)
            {
                var feedback = new ReportFeedback
                {
                    Name = model.Name,
                    Email = model.Email,
                    Message = model.Message,
                    SentAt = DateTime.Now,
                    UserId = User.Identity.IsAuthenticated
                        ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                        : null
                };

                _context.ReportFeedbacks.Add(feedback);
                _context.SaveChanges();

                TempData["Success"] = "Cảm ơn bạn đã gửi phản hồi. Chúng tôi sẽ liên hệ lại sớm nhất có thể.";
                return RedirectToAction("Index", "Home");
            }

            TempData["Error"] = "Thông tin không hợp lệ. Vui lòng kiểm tra lại.";
            return RedirectToAction("Index", "Home");
        }




        public IActionResult ReportList()
        {
            var reports = _context.ReportFeedbacks
                .OrderByDescending(r => r.SentAt)
                .ToList();

            return View(reports);
        }

        [HttpGet]
        public IActionResult Reply(int id)
        {
            var feedback = _context.ReportFeedbacks.Find(id);
            if (feedback == null) return NotFound();
            return View(feedback); // chỉ hiển thị form phản hồi
        }

        [HttpPost]
        public IActionResult Reply(int id, string AdminReply)
        {
            var feedback = _context.ReportFeedbacks.Find(id);
            if (feedback == null) return NotFound();

            feedback.AdminReply = AdminReply;
            feedback.RepliedAt = DateTime.Now;
            _context.SaveChanges();

            // Tạo thông báo cho khách hàng
            var userNotification = new UserNotification
            {
                UserId = feedback.UserId, // đảm bảo bạn đã lưu UserId vào ReportFeedback
                Message = $"Phản hồi từ Admin: {AdminReply}",
                CreatedAt = DateTime.Now,
                IsRead = false
            };
            _context.UserNotifications.Add(userNotification);
            _context.SaveChanges();

            TempData["Success"] = "Đã phản hồi cho người dùng và gửi thông báo.";
            return RedirectToAction("ReportList");
        }

        // Add to DashboardController.cs
        public IActionResult DesignRequests()
        {
            var designRequests = _context.designRequest
                .Include(d => d.Product)
                .OrderByDescending(d => d.CreatedDate)
                .ToList();

            return View(designRequests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendDesignSample(int id, IFormFile designSample)
        {
            var designRequest = await _context.designRequest.FindAsync(id);
            if (designRequest == null)
            {
                return NotFound();
            }

            if (designSample != null && designSample.Length > 0)
            {
                // Save the file
                var uploadsFolder = Path.Combine("wwwroot", "uploads", "design-samples");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + designSample.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await designSample.CopyToAsync(fileStream);
                }

                // Update the design request
                designRequest.AdminFeedbackImage = "/uploads/design-samples/" + uniqueFileName;
                designRequest.Status = "Đã gửi mẫu";
                designRequest.FeedbackDate = DateTime.Now;

                _context.Update(designRequest);
                await _context.SaveChangesAsync();

                // Create notification for customer
                var notification = new UserNotification
                {
                    UserId = designRequest.UserId,
                    Message = $"Admin đã gửi mẫu thiết kế cho yêu cầu #{designRequest.Id}",
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    // RelatedUrl = $"/Design/Details/{designRequest.Id}"
                };
                _context.UserNotifications.Add(notification);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đã gửi mẫu thiết kế thành công!";
            }
            else
            {
                TempData["Error"] = "Vui lòng chọn file để gửi";
            }

            return RedirectToAction(nameof(DesignRequests));
        }

public IActionResult GetDesignDetails(int id)
{
    var designRequest = _context.designRequest
        .Include(d => d.Product)
        .FirstOrDefault(d => d.Id == id);

    if (designRequest == null)
    {
        return NotFound();
    }

    return PartialView("_DesignDetailsPartial", designRequest);
}





    }
}
