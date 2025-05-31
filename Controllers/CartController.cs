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

        public CartController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager, 
            ILogger<CartController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Cart/Index
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

        // POST: Cart/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return RedirectToAction("Login", "Account");

                var product = await _context.Products.FindAsync(productId);
                if (product == null) return NotFound();

                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == user.Id);

                if (cart == null)
                {
                    cart = new Cart { UserId = user.Id };
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync();
                }

                var item = cart.Items?.FirstOrDefault(i => i.ProductId == productId);
                if (item != null)
                {
                    item.Quantity += quantity;
                }
                else
                {
                    item = new ItemCart 
                    { 
                        ProductId = productId, 
                        Quantity = quantity, 
                        CartId = cart.Id 
                    };
                    _context.ItemCarts.Add(item);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã thêm sản phẩm vào giỏ hàng!";
                return RedirectToAction("Index", "Product");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product to cart");
                TempData["Error"] = "Có lỗi xảy ra khi thêm sản phẩm vào giỏ hàng";
                return RedirectToAction("Index", "Product");
            }
        }

        // POST: Cart/Remove
        [HttpPost]
        public async Task<IActionResult> Remove(int itemId)
        {
            try
            {
                var item = await _context.ItemCarts.FindAsync(itemId);
                if (item != null)
                {
                    _context.ItemCarts.Remove(item);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Đã xóa sản phẩm khỏi giỏ hàng";
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cart item");
                TempData["Error"] = "Có lỗi xảy ra khi xóa sản phẩm";
                return RedirectToAction("Index");
            }
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int itemId, int quantity)
        {
            try
            {
                if (quantity <= 0) return BadRequest("Số lượng phải lớn hơn 0.");

                var item = await _context.ItemCarts.FindAsync(itemId);
                if (item == null) return NotFound();

                item.Quantity = quantity;
                _context.Update(item);
                await _context.SaveChangesAsync();
                
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating quantity");
                return StatusCode(500, "Có lỗi xảy ra khi cập nhật số lượng");
            }
        }

        // GET: Cart/GetCartSummary
        [HttpGet]
        public async Task<IActionResult> GetCartSummary()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return PartialView("_EmptyCart");

                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(c => c.UserId == user.Id);

                var cartItems = cart?.Items.Select(i => new
                {
                    ProductName = i.Product?.Name,
                    ProductImage = i.Product?.ImageUrl,
                    Quantity = i.Quantity,
                    TotalPrice = (i.Product?.Price ?? 0) * i.Quantity
                }).ToList();

                return Json(cartItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart summary");
                return PartialView("_EmptyCart");
            }
        }

        // GET: Cart/GetCartItems
        [HttpGet]
        public async Task<IActionResult> GetCartItems()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { items = new List<object>(), totalItems = 0, totalPrice = 0 });
                }

                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(c => c.UserId == user.Id);

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart items");
                return Json(new { items = new List<object>(), totalItems = 0, totalPrice = 0 });
            }
        }

        // GET: Cart/Checkout
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || !cart.Items.Any())
                {
                    TempData["Error"] = "Giỏ hàng của bạn đang trống";
                    return RedirectToAction("Index");
                }

                var user = await _userManager.FindByIdAsync(userId);
                var model = new CheckoutViewModel
                {
                    FullName = user?.FullName ?? "",
                    Address = user?.Address ?? "",
                    PhoneNumber = user?.PhoneNumber ?? "",
                    Items = cart.Items.ToList(),
                    SubTotal = cart.Items.Sum(i => i.Product.Price * i.Quantity),
                    ShippingFee = 30000m, // Fixed shipping fee
                    DesignFee = 50000m    // Fixed design fee
                };

                model.CalculateTotals(); // Calculate total amount

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading checkout page");
                TempData["Error"] = "Có lỗi xảy ra khi tải trang thanh toán";
                return RedirectToAction("Index");
            }
        }

        // POST: Cart/Checkout
        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Checkout ModelState invalid");
                    return View(model);
                }

                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || !cart.Items.Any())
                {
                    TempData["Error"] = "Giỏ hàng rỗng.";
                    return RedirectToAction("Index");
                }

                // Calculate order totals
                decimal subTotal = cart.Items.Sum(i => i.Product.Price * i.Quantity);
                decimal shippingFee = 30000m;
                decimal designFee = 50000m;
                decimal totalAmount = subTotal + shippingFee + designFee;

                // Create new order
                var hoaDon = new HoaDon
                {
                    UserId = userId,
                    TenNguoiNhan = model.FullName,
                    DiaChi = model.Address,
                    SoDienThoai = model.PhoneNumber,
                    SubTotal = subTotal,
                    ShippingFee = shippingFee,
                    DesignFee = designFee,
                    TongTien = totalAmount,
                    PaymentMethod = model.PaymentMethod,
                    Status = model.PaymentMethod == PaymentMethod.COD ? "Chờ xác nhận" : "Chờ thanh toán",
                    NgayLap = DateTime.Now
                };

                _context.HoaDons.Add(hoaDon);
                await _context.SaveChangesAsync();

                // Add order details
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

                // Clear cart
                _context.ItemCarts.RemoveRange(cart.Items);
                _context.Carts.Remove(cart);
                await _context.SaveChangesAsync();

                // Prepare model for redirection
                model.Id = hoaDon.Id;
                model.SubTotal = subTotal;
                model.ShippingFee = shippingFee;
                model.DesignFee = designFee;
                model.TotalAmount = totalAmount;
                model.ChiTietHoaDon = hoaDon.ChiTietHoaDon.ToList();

                // Redirect based on payment method
                return model.PaymentMethod switch
                {
                    PaymentMethod.BankTransfer => RedirectToAction("BankInstructions", model),
                    PaymentMethod.MoMo => RedirectToAction("MoMoRedirect", model),
                    _ => RedirectToAction("OrderSuccess", model)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during checkout");
                TempData["Error"] = "Có lỗi xảy ra trong quá trình thanh toán";
                return View(model);
            }
        }

        // GET: Cart/BankInstructions
        [HttpGet]
        public async Task<IActionResult> BankInstructions(CheckoutViewModel model)
        {
            try
            {
                if (model.Id == 0)
                {
                    var hoaDon = await _context.HoaDons
                        .Include(h => h.ChiTietHoaDon)
                        .ThenInclude(ct => ct.Product)
                        .FirstOrDefaultAsync(h => h.Id == model.Id);
                    
                    if (hoaDon != null)
                    {
                        model.SubTotal = hoaDon.SubTotal;
                        model.ShippingFee = hoaDon.ShippingFee;
                        model.DesignFee = hoaDon.DesignFee;
                        model.TotalAmount = hoaDon.TongTien;
                        model.ChiTietHoaDon = hoaDon.ChiTietHoaDon.ToList();
                    }
                }
                
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading bank instructions");
                TempData["Error"] = "Có lỗi xảy ra khi tải hướng dẫn thanh toán";
                return RedirectToAction("Index");
            }
        }

        // GET: Cart/MoMoRedirect
        [HttpGet]
        public async Task<IActionResult> MoMoRedirect(CheckoutViewModel model)
        {
            try
            {
                if (model.Id == 0)
                {
                    var hoaDon = await _context.HoaDons
                        .Include(h => h.ChiTietHoaDon)
                        .ThenInclude(ct => ct.Product)
                        .FirstOrDefaultAsync(h => h.Id == model.Id);
                    
                    if (hoaDon != null)
                    {
                        model.SubTotal = hoaDon.SubTotal;
                        model.ShippingFee = hoaDon.ShippingFee;
                        model.DesignFee = hoaDon.DesignFee;
                        model.TotalAmount = hoaDon.TongTien;
                        model.ChiTietHoaDon = hoaDon.ChiTietHoaDon.ToList();
                    }
                }
                
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading MoMo redirect");
                TempData["Error"] = "Có lỗi xảy ra khi tải trang thanh toán MoMo";
                return RedirectToAction("Index");
            }
        }

        // GET: Cart/OrderSuccess
        [HttpGet]
        public async Task<IActionResult> OrderSuccess(CheckoutViewModel model)
        {
            try
            {
                if (model.Id == 0)
                {
                    var hoaDon = await _context.HoaDons
                        .Include(h => h.ChiTietHoaDon)
                        .ThenInclude(c => c.Product)
                        .FirstOrDefaultAsync(h => h.Id == model.Id);
                    
                    if (hoaDon != null)
                    {
                        model.SubTotal = hoaDon.SubTotal;
                        model.ShippingFee = hoaDon.ShippingFee;
                        model.DesignFee = hoaDon.DesignFee;
                        model.TotalAmount = hoaDon.TongTien;
                        model.ChiTietHoaDon = hoaDon.ChiTietHoaDon.ToList();
                        model.PaymentMethod = hoaDon.PaymentMethod;
                    }
                }
                
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order success");
                TempData["Error"] = "Có lỗi xảy ra khi tải trang xác nhận đơn hàng";
                return RedirectToAction("Index");
            }
        }
    }
}