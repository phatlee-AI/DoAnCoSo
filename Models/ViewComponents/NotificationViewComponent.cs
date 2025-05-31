using Microsoft.AspNetCore.Mvc;
using MyWebApp.Models;
using System.Security.Claims;
using System.Linq;
using Microsoft.EntityFrameworkCore;

public class NotificationViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public NotificationViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return View(Enumerable.Empty<UserNotification>());

        var notifications = await _context.UserNotifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(10) // Increased from 5 to 10 for better UX
            .ToListAsync();

        var unreadCount = notifications.Count(n => !n.IsRead);
        ViewBag.UnreadCount = unreadCount;
        ViewBag.RecentNotifications = notifications; // Pass the full list to view

        return View(notifications);
    }
}