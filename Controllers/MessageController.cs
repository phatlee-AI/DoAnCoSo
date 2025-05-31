using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebApp.Models;
using System.Security.Claims;

[Authorize]
public class MessageController : Controller
{
    private readonly ApplicationDbContext _context;

    public MessageController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Chat(string withUserId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Lấy người đang trò chuyện cùng
        var withUser = _context.Users.FirstOrDefault(u => u.Id == withUserId);
        if (withUser == null) return NotFound();

        // Lấy danh sách tin nhắn giữa hai người
        var messages = _context.Messages
            .Where(m =>
                (m.FromUserId == currentUserId && m.ToUserId == withUserId) ||
                (m.FromUserId == withUserId && m.ToUserId == currentUserId))
            .OrderBy(m => m.SentAt)
            .ToList();

        // Gán dữ liệu cho view
        ViewBag.WithUser = withUser;
        ViewBag.WithUserId = withUserId;

        var model = new Tuple<ApplicationUser, List<Message>>(withUser, messages);
        return View(model);
    }

    [HttpPost]
    public IActionResult Send(string toUserId, string content)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var message = new Message
        {
            FromUserId = currentUserId,
            ToUserId = toUserId,
            Content = content,
            SentAt = DateTime.Now,
            IsRead = false
        };

        _context.Messages.Add(message);
        _context.SaveChanges();

        return RedirectToAction("Chat", new { withUserId = toUserId });
    }
}
