using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebApp.Models;

public class FeaturedProductsViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public FeaturedProductsViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Take(4)
            .ToListAsync();

        return View(products);
    }
}
