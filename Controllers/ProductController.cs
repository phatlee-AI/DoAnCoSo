using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyWebApp.Models;
using System.Collections;
using Microsoft.AspNetCore.Authorization;
using MyWebApp.Models;

namespace MyWebApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Product
        public async Task<IActionResult> Index(string timkiem="")
        {
            string z = MyWebApp.BoTRo.Filter.ChuyenCoDauThanhKhongDau(timkiem);
            if(timkiem == null || timkiem == "")
            {
                var applicationDbContext = _context.Products.Include(p => p.Category);
                ViewBag.timkiem = timkiem;
                return View(await applicationDbContext.ToListAsync());
               
            }
            else
            {

                IEnumerable<Product> dstimkiem = _context.Products.Include(p => p.Category);
                List<Product> ds = new List<Product>();
                foreach(var i in dstimkiem)
                {
                    string a1 =(i.Description.ToUpper());
                    if (a1.ToUpper().Contains(z.ToUpper()))
                    {
                        ds.Add(i);
                        continue;
                    }
                    string a2 = (i.Name.ToUpper());
                    if (a2.ToUpper().Contains(z.ToUpper()))
                    {
                        ds.Add(i);
                        continue;
                    }
                    string a3 = (i.Category.Name.ToUpper());
                    if (a3.ToUpper().Contains(timkiem.ToUpper()))
                    {
                        ds.Add(i);
                        continue;
                    }
                }
                ViewBag.timkiem = timkiem;
                return View(ds);
            }
            
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            List<ProductImage> dshinhanh = await _context.ProductImages.Where(i => i.ProductId == id).ToListAsync();
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.dshinhanh = dshinhanh;
            return View(product);
        }

        public async Task<string> SaveImage(IFormFile ImageURL, string subFolder)
        {
            if (ImageURL == null || ImageURL.Length == 0)
            {
                throw new ArgumentException("File không hợp lệ!");
            }

            // Đường dẫn thư mục lưu ảnh trong wwwroot/uploads/tin-tuc
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", subFolder);

            // Tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Tạo tên file duy nhất để tránh trùng lặp
            string fileExtension = Path.GetExtension(ImageURL.FileName);
            string fileName = Path.GetFileNameWithoutExtension(ImageURL.FileName);
            string uniqueFileName = fileName + "_" + Guid.NewGuid().ToString("N") + fileExtension;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Lưu file vào thư mục
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await ImageURL.CopyToAsync(fileStream);
            }

            // Trả về đường dẫn tương đối để hiển thị ảnh trên web
            return $"/uploads/{subFolder}/{uniqueFileName}";
        }


        public void DeleteImage(string ImageURL, string subFolder)
        {
            if (string.IsNullOrEmpty(ImageURL))
            {
                throw new ArgumentException("Đường dẫn xóa ảnh không hợp lệ!");
            }

            // Lấy đường dẫn tuyệt đối của ảnh trong thư mục wwwroot/uploads/
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", subFolder);
            string filePath = Path.Combine(uploadsFolder, Path.GetFileName(ImageURL));

            // Kiểm tra nếu file tồn tại thì xóa
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        // GET: Product/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Product/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Product product, IFormFile? HinhAnhCapNhat, List<IFormFile>? DanhSachHinhAnh, IFormFile? model3dFile)
{
    try
    {
        if (ModelState.IsValid)
        {
            if (HinhAnhCapNhat != null)
            {
                product.ImageUrl = await SaveImage(HinhAnhCapNhat, "SanPham");
            }

            if (model3dFile != null)
            {
                product.Model3DUrl = await SaveImage(model3dFile, "SanPham");
            }

            _context.Add(product);
            await _context.SaveChangesAsync();

            if (DanhSachHinhAnh != null)
            {
                foreach (IFormFile i in DanhSachHinhAnh)
                {
                    ProductImage productImage = new ProductImage
                    {
                        ProductId = product.Id,
                        Url = await SaveImage(i, "SanPham")
                    };
                    _context.ProductImages.Add(productImage);
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
    catch (Exception ex)
    {
        ModelState.AddModelError("", $"Lỗi tạo sản phẩm: {ex.Message}");
    }

    ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
    return View(product);
}



        // GET: Product/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Product/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(Product product, IFormFile? HinhAnhCapNhat, List<IFormFile>? DanhSachHinhAnh)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    Product z = _context.Products.FirstOrDefault(i => i.Id == product.Id);
                    if(HinhAnhCapNhat != null)
                    {
                        if (product.ImageUrl != null && product.ImageUrl != "")
                        {
                            DeleteImage(z.ImageUrl, "SanPham");
                        }
                        
                        z.ImageUrl = await SaveImage(HinhAnhCapNhat, "SanPham");
                    }
                    z.CategoryId = product.CategoryId;
                    z.Name = product.Name;
                    z.Description = product.Description;
                    z.Price = product.Price;
                    _context.Products.Update(z);
                    await _context.SaveChangesAsync();

                    if(DanhSachHinhAnh!=null)
                    {
                        List<ProductImage> ds=  _context.ProductImages.Where(i=>i.ProductId == z.Id).ToList();
                        if(ds!=null)
                        {
                            foreach (ProductImage image in ds)
                            {
                                _context.ProductImages.Remove(image);
                                await _context.SaveChangesAsync();

                            }
                        }
                        foreach (IFormFile i in DanhSachHinhAnh)
                        {
                            ProductImage productImage = new ProductImage();
                            productImage.ProductId = product.Id;
                            productImage.Url = await SaveImage(i, "SanPham");
                            _context.ProductImages.Add(productImage);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                if(product.ImageUrl != null)
                {
                    DeleteImage(product.ImageUrl, "SanPham");
                }
                List<ProductImage> DanhSachHinhAnh = _context.ProductImages.Where(i => i.ProductId == id).ToList();
                if(DanhSachHinhAnh != null)
                {
                    foreach (ProductImage i in DanhSachHinhAnh)
                    {
                        DeleteImage(i.Url, "SanPham");
                        _context.ProductImages.Remove(i);
                        await _context.SaveChangesAsync();
                    }

                }
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
