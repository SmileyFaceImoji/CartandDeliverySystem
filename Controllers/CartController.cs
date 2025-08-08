using CartandDeliverySystem.Data;
using CartandDeliverySystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace CartandDeliverySystem.Controllers
{
    public class CartController : Controller
    {
        private readonly CartService _cart;
        private readonly ApplicationDbContext _context;

        public CartController(CartService cart, ApplicationDbContext context)
        {
            _cart = cart;
            _context = context;
        }
        public IActionResult Index()
        {
            var items = _cart.GetItems();
            return View(items ?? new List<CartItem>());
        }

        public async Task<IActionResult> AddToCart(int ProductId, int quantity)
        {
            var theProd = await _context.Products.FindAsync(ProductId);

            if (theProd == null)
            {
                return NotFound();
            }
             _cart.AddItem(theProd, quantity);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var removed = _cart.RemoveItem(productId);

            if (removed)
            {
                TempData["SuccessMessage"] = "Item removed from cart";
            }
            else
            {
                TempData["ErrorMessage"] = "Item not found in cart";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            // Validate quantity
            if (quantity < 0)
            {
                TempData["ErrorMessage"] = "Quantity cannot be negative";
                return RedirectToAction(nameof(Index));
            }

            // Check product exists
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found";
                return RedirectToAction("Index");
            }

            // Check stock availability
            if (quantity > product.Stock)
            {
                TempData["ErrorMessage"] = $"Only {product.Stock} items available";
                return RedirectToAction("Index");
            }

            // Update cart
            var updated = _cart.UpdateQuantity(productId, quantity);

            if (updated)
            {
                if (quantity == 0)
                {
                    TempData["SuccessMessage"] = $"{product.Name} removed from cart";
                }
                else
                {
                    TempData["SuccessMessage"] = $"{product.Name} quantity updated to {quantity}";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Item not found in cart";
            }

            return RedirectToAction("Index");
        }
    }
}
