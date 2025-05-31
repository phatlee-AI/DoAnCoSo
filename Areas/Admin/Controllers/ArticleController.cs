using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using X.PagedList;
using X.PagedList.Extensions;

namespace MyWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ArticleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private const int PageSize = 10;

        public ArticleController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: Admin/Article
        public async Task<IActionResult> Index(
    string sortOrder,
    string currentFilter,
    string searchString,
    string status,
    int? page)
{
    ViewData["CurrentSort"] = sortOrder;
    ViewData["TitleSort"] = string.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
    ViewData["DateSort"] = sortOrder == "date_asc" ? "date_desc" : "date_asc";

    if (searchString != null)
    {
        page = 1;
    }
    else
    {
        searchString = currentFilter;
    }

    ViewData["CurrentFilter"] = searchString;
    ViewData["StatusFilter"] = status;

    var articles = _context.Article
        .Include(a => a.Author)
        .AsQueryable();

    if (!string.IsNullOrEmpty(searchString))
    {
        articles = articles.Where(a =>
            a.Title.Contains(searchString) ||
            a.Content.Contains(searchString) ||
            a.Summary.Contains(searchString));
    }

    if (!string.IsNullOrEmpty(status))
    {
        var isPublished = status == "published";
        articles = articles.Where(a => a.IsPublished == isPublished);
    }

    articles = sortOrder switch
    {
        "title_desc" => articles.OrderByDescending(a => a.Title),
        "title_asc" => articles.OrderBy(a => a.Title),
        "date_asc" => articles.OrderBy(a => a.CreatedDate),
        _ => articles.OrderByDescending(a => a.CreatedDate),
    };

    int pageNumber = page ?? 1;

    // ✅ KHÔNG cần await ở đây vì ToPagedListAsync là extension method trả Task<IPagedList<T>>
    var pagedList = articles.ToPagedList(pageNumber, PageSize);

    return View(pagedList);
}


        // Các action khác giữ nguyên...
        // GET: Admin/Article/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Article
                .Include(a => a.Author)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }

        // GET: Admin/Article/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Article/Create
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create([Bind("Id,Title,Content,Summary,IsPublished,Slug,MetaTitle,MetaDescription,Tags")] Article article)
{
    try
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);
            article.AuthorId = user.Id;
            article.CreatedDate = DateTime.Now;
            
            // Xử lý upload ảnh với try-catch
            if (Request.Form.Files.Count > 0)
            {
                var file = Request.Form.Files[0];
                if (file.Length > 0)
                {
                    try 
                    {
                        article.ImageUrl = await UploadImage(file);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Error uploading image: {ex.Message}");
                        return View(article);
                    }
                }
            }

            // Tạo slug nếu trống
            if (string.IsNullOrEmpty(article.Slug))
            {
                article.Slug = GenerateSlug(article.Title);
            }

            _context.Article.Add(article);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Article created successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
    catch (Exception ex)
    {
        ModelState.AddModelError("", $"An error occurred: {ex.Message}");
    }
    
    // Nếu có lỗi, hiển thị lại form với thông báo
    return View(article);
}

        // GET: Admin/Article/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Article.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }
            return View(article);
        }

        // POST: Admin/Article/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,Summary,IsPublished,Slug,MetaTitle,MetaDescription,Tags,AuthorId,CreatedDate,ImageUrl")] Article article)
        {
            if (id != article.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    article.UpdatedDate = DateTime.Now;
                    
                    // Handle image upload
                    if (Request.Form.Files.Count > 0)
                    {
                        var file = Request.Form.Files[0];
                        if (file.Length > 0)
                        {
                            // Delete old image if exists
                            if (!string.IsNullOrEmpty(article.ImageUrl))
                            {
                                DeleteImage(article.ImageUrl);
                            }
                            article.ImageUrl = await UploadImage(file);
                        }
                    }
                    // Handle image deletion if checkbox is checked
                    else if (Request.Form["deleteImage"] == "true")
                    {
                        if (!string.IsNullOrEmpty(article.ImageUrl))
                        {
                            DeleteImage(article.ImageUrl);
                            article.ImageUrl = null;
                        }
                    }

                    _context.Article.Update(article);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Article updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArticleExists(article.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(article);
        }

        // POST: Admin/Article/TogglePublish/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublish(int id)
        {
            var article = await _context.Article.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            article.IsPublished = !article.IsPublished;
            article.UpdatedDate = DateTime.Now;
            _context.Article.Update(article);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Article {(article.IsPublished ? "published" : "unpublished")} successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Article/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Article
                .Include(a => a.Author)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }

        // POST: Admin/Article/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var article = await _context.Article.FindAsync(id);
            
            // Delete associated image
            if (!string.IsNullOrEmpty(article.ImageUrl))
            {
                DeleteImage(article.ImageUrl);
            }

            _context.Article.Remove(article);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Article deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool ArticleExists(int id)
        {
            return _context.Article.Any(e => e.Id == id);
        }

        private async Task<string> UploadImage(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "articles");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/uploads/articles/" + uniqueFileName;
        }

        private void DeleteImage(string imageUrl)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                var imagePath = Path.Combine(_hostingEnvironment.WebRootPath, imageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
        }

        private string GenerateSlug(string title)
        {
            return title.ToLower()
                .Replace(" ", "-")
                .Replace(".", "")
                .Replace(",", "")
                .Replace("!", "")
                .Replace("?", "")
                .Replace(":", "")
                .Replace(";", "")
                .Replace("&", "and")
                .Replace("--", "-")
                .Trim('-');
        }
    }
}