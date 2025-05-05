using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebApp.Models;
using System.Diagnostics;

namespace MyWebApp.Controllers
{
    public class DesignController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DesignController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Design/Create?productId=1
        public IActionResult Create(int? productId)
{
    if (productId == null)
        return NotFound();

    var product = _context.Products.FirstOrDefault(p => p.Id == productId);
    if (product == null)
        return NotFound();

    ViewBag.Product = product;
    var model = new DesignRequest { ProductId = product.Id };
    return View(model);
}


        // POST: Design/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DesignRequest designRequest)
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload hình ảnh nếu có
                if (Request.Form.Files.Count > 0)
                {
                    var file = Request.Form.Files[0];
                    if (file.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "designs");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        designRequest.ImagePath = "/designs/" + uniqueFileName;
                    }
                }

                _context.designRequest.Add(designRequest);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Yêu cầu thiết kế của bạn đã được gửi thành công!";
                return RedirectToAction(nameof(MyDesigns));
            }

            ViewBag.Product = _context.Products.Find(designRequest.ProductId);
            return View(designRequest);
        }

        // GET: Design/MyDesigns
        public IActionResult MyDesigns()
        {
            // Trong thực tế, bạn sẽ lọc theo user đăng nhập
            var designs = _context.designRequest
                .Include(d => d.Product)
                .OrderByDescending(d => d.CreatedDate)
                .ToList();
                
            return View(designs);
        }

        // GET: Design/Details/5
        public IActionResult Details(int id)
        {
            var designRequest = _context.designRequest
                .Include(d => d.Product)
                .FirstOrDefault(d => d.Id == id);
                
            if (designRequest == null)
            {
                return NotFound();
            }

            return View(designRequest);
        }
    }
}