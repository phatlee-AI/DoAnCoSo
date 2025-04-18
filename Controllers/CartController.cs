using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyWebApp.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CartController> _logger;

        public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<CartController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            var cart = await _context.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new Cart { UserId = user.Id };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var item = cart.Items?.FirstOrDefault(i => i.ProductId == productId);
            if (item != null) item.Quantity += quantity;
            else
            {
                item = new ItemCart { ProductId = productId, Quantity = quantity, CartId = cart.Id };
                _context.ItemCarts.Add(item);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã thêm sản phẩm vào giỏ hàng!";
            return RedirectToAction("Index", "Product");
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int itemId)
        {
            var item = await _context.ItemCarts.FindAsync(itemId);
            if (item != null)
            {
                _context.ItemCarts.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int itemId, int quantity)
        {
            if (quantity <= 0) return BadRequest("Số lượng phải lớn hơn 0.");

            var item = await _context.ItemCarts.FindAsync(itemId);
            if (item != null)
            {
                item.Quantity = quantity;
                _context.Update(item);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetCartSummary()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return PartialView("_EmptyCart");

            var cart = await _context.Carts.Include(c => c.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(c => c.UserId == user.Id);

            var cartItems = cart?.Items.Select(i => new
            {
                ProductName = i.Product?.Name,
                ProductImage = i.Product?.ImageUrl,
                Quantity = i.Quantity,
                TotalPrice = (i.Product?.Price ?? 0) * i.Quantity
            }).ToList();

            return Json(cartItems);
        }

        [HttpGet]
        public async Task<IActionResult> GetCartItems()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { items = new List<object>(), totalItems = 0, totalPrice = 0 });
            }

            var cart = await _context.Carts.Include(c => c.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || cart.Items == null || !cart.Items.Any())
            {
                return Json(new { items = new List<object>(), totalItems = 0, totalPrice = 0 });
            }

            var cartItems = cart.Items.Select(item => new
            {
                productId = item.ProductId,
                productName = item.Product.Name,
                price = item.Product.Price,
                quantity = item.Quantity,
                imageUrl = item.Product.ImageUrl,
                totalPrice = item.Product.Price * item.Quantity
            }).ToList();

            var totalItems = cart.Items.Sum(i => i.Quantity);
            var totalPrice = cart.Items.Sum(i => i.Product.Price * i.Quantity);

            return Json(new { items = cartItems, totalItems, totalPrice });
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = _userManager.GetUserId(User);
            var cart = await _context.Carts.Include(c => c.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return RedirectToAction("Index", "Cart");
            }

            var model = new CheckoutViewModel
            {
                FullName = "",
                Address = "",
                PhoneNumber = "",
                Items = cart.Items.ToList(),
                TotalAmount = cart.Items.Sum(i => i.Product.Price * i.Quantity)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Checkout ModelState invalid");
                return View(model);
            }

            var userId = _userManager.GetUserId(User);
            var cart = await _context.Carts.Include(c => c.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || cart.Items == null || !cart.Items.Any())
            {
                TempData["Error"] = "Giỏ hàng rỗng.";
                return RedirectToAction("Index");
            }

            decimal totalAmount = cart.Items.Sum(i => i.Product.Price * i.Quantity);

            var hoaDon = new HoaDon
            {
                UserId = userId,
                TenNguoiNhan = model.FullName,
                DiaChi = model.Address,
                SoDienThoai = model.PhoneNumber,
                TongTien = totalAmount,
                PaymentMethod = model.PaymentMethod,
                Status = model.PaymentMethod == PaymentMethod.COD ? "Chờ xác nhận" : "Chờ thanh toán",
                NgayLap = DateTime.Now
            };

            _context.HoaDons.Add(hoaDon);
            await _context.SaveChangesAsync();

            foreach (var item in cart.Items)
            {
                var chiTiet = new HoaDonChiTiet
                {
                    HoaDonId = hoaDon.Id,
                    ProductId = item.ProductId,
                    SoLuong = item.Quantity,
                    DonGia = item.Product.Price
                };
                _context.HoaDonChiTiets.Add(chiTiet);
            }

            await _context.SaveChangesAsync();
            _context.ItemCarts.RemoveRange(cart.Items);
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();

            return model.PaymentMethod switch
            {
                PaymentMethod.BankTransfer => RedirectToAction("BankInstructions", new { id = hoaDon.Id }),
                PaymentMethod.MoMo => RedirectToAction("MoMoRedirect", new { id = hoaDon.Id }),
                _ => RedirectToAction("OrderSuccess", new { id = hoaDon.Id })
            };
        }

        [HttpGet]
        public async Task<IActionResult> BankInstructions(int id)
        {
            var hoaDon = await _context.HoaDons.Include(h => h.ChiTietHoaDon).ThenInclude(ct => ct.Product).FirstOrDefaultAsync(h => h.Id == id);
            if (hoaDon == null) return NotFound();

            var model = new CheckoutViewModel
            {
                Id = hoaDon.Id,
                FullName = hoaDon.TenNguoiNhan,
                Address = hoaDon.DiaChi,
                PhoneNumber = hoaDon.SoDienThoai,
                TotalAmount = hoaDon.TongTien,
                ChiTietHoaDon = hoaDon.ChiTietHoaDon.ToList()
            };

            return View("BankInstructions", model);
        }

        [HttpGet]
        public async Task<IActionResult> MoMoRedirect(int id)
        {
            var hoaDon = await _context.HoaDons.Include(h => h.ChiTietHoaDon).ThenInclude(ct => ct.Product).FirstOrDefaultAsync(h => h.Id == id);
            if (hoaDon == null) return NotFound();

            var model = new CheckoutViewModel
            {
                Id = hoaDon.Id,
                FullName = hoaDon.TenNguoiNhan,
                Address = hoaDon.DiaChi,
                PhoneNumber = hoaDon.SoDienThoai,
                TotalAmount = hoaDon.TongTien,
                ChiTietHoaDon = hoaDon.ChiTietHoaDon.ToList()
            };

            return View("MoMoRedirect", model);
        }

        [HttpGet]
        public async Task<IActionResult> OrderSuccess(int id)
        {
            var hoaDon = await _context.HoaDons.Include(h => h.ChiTietHoaDon).ThenInclude(c => c.Product).FirstOrDefaultAsync(h => h.Id == id);
            if (hoaDon == null) return NotFound();

            var model = new CheckoutViewModel
            {
                Id = hoaDon.Id,
                FullName = hoaDon.TenNguoiNhan,
                Address = hoaDon.DiaChi,
                PhoneNumber = hoaDon.SoDienThoai,
                TotalAmount = hoaDon.TongTien,
                PaymentMethod = hoaDon.PaymentMethod,
                ChiTietHoaDon = hoaDon.ChiTietHoaDon.ToList()
            };

            return View("OrderSuccess", model);
        }
    }
}
