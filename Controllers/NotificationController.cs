using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using MyWebApp.Models;
using Microsoft.EntityFrameworkCore;

public class NotificationController : Controller
{
    private readonly ApplicationDbContext _context;
    private dynamic recentNotifications;

    public NotificationController(ApplicationDbContext context)
    {
        _context = context;
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public IActionResult Index()
    {
        // Lấy số lượng thông báo chưa đọc từ database
        var unreadCount = _context.UserNotifications.Count(n => !n.IsRead);

        // Lấy 5 thông báo gần nhất
        var recentNotifications = _context.UserNotifications
            .OrderByDescending(n => n.CreatedAt)
            .Take(5)
            .ToList();

        ViewBag.UnreadCount = unreadCount;
        ViewBag.RecentNotifications = recentNotifications;

        return View();
    }

    [HttpGet]
    public IActionResult GetCount()
    {
        var userId = GetCurrentUserId(); // Tùy hệ thống bạn lấy userId sao cho đúng
        int count = _context.UserNotifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .Count();

        return Json(count);
    }

    [HttpGet]
public async Task<IActionResult> GetUnreadCount()
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (userId == null) return Json(0);

    var count = await _context.UserNotifications
        .CountAsync(n => n.UserId == userId && !n.IsRead);
        
    return Json(count);
}

[HttpGet]
public async Task<IActionResult> GetRecentNotifications()
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (userId == null) return Json(new List<object>());

    var notifications = await _context.UserNotifications
        .Where(n => n.UserId == userId)
        .OrderByDescending(n => n.CreatedAt)
        .Take(10)
        .Select(n => new {
            id = n.Id,
            // title = n.Title,
            message = n.Message,
            // type = n.Type,
            isRead = n.IsRead,
            createdAt = n.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss")
        })
        .ToListAsync();

    return Json(notifications);
}

}
