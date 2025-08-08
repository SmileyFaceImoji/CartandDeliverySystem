using CartandDeliverySystem.Data;
using CartandDeliverySystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CartandDeliverySystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var prod = await _context.Products.ToListAsync();
            return View(prod);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                    // Log the error (uncomment ex variable name and write a log)
                }
            }

            // If we got this far, something failed; redisplay form
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? Id)
        {
            if (Id == null) 
                return NotFound();

            var TheProd = await _context.Products
                .FindAsync(Id);

            if (TheProd == null)
                return RedirectToAction(nameof(Index));

            return View(TheProd);
        }
    
    }
}
