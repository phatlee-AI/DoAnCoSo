using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MyWebApp.Areas.Admin.Models.ViewModels;
using MyWebApp.Models;

namespace MyWebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context,ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
}

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult AboutUs()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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


}
