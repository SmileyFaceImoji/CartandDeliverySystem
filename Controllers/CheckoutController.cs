using CartandDeliverySystem.Data;
using CartandDeliverySystem.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CartandDeliverySystem.Models;

namespace CartandDeliverySystem.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;
        private readonly string _stripeSecretKey;

        public CheckoutController(ApplicationDbContext context, CartService cartService, IConfiguration configuration)
        {
            _context = context;
            _cartService = cartService;
            _stripeSecretKey = configuration["Stripe:SecretKey"];
            StripeConfiguration.ApiKey = _stripeSecretKey;
        }

        [HttpGet]
        public IActionResult ConfirmAddress()
        {
            var model = new ShippingViewModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult ConfirmAddress(ShippingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            HttpContext.Session.SetObject("ShippingInfo", model);
            return RedirectToAction("Pay");
        }

        public IActionResult Pay()
        {
            StripeConfiguration.ApiKey = _stripeSecretKey;

            var cartItems = _cartService.GetItems().ToList();
            if (!cartItems.Any()) return RedirectToAction("Index", "Cart");

            var shippingInfo = HttpContext.Session.GetObject<ShippingViewModel>("ShippingInfo");
            if (shippingInfo == null) return RedirectToAction("ConfirmAddress");

            var domain = "https://localhost:7155"; // Replace with your live domain in production

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" }, // FIX: Ensure card payments
                LineItems = cartItems.Select(item => new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Name,
                        },
                    },
                    Quantity = item.Quantity,
                }).ToList(),
                Mode = "payment",
                SuccessUrl = domain + "/Checkout/Success",
                CancelUrl = domain + "/Checkout/Cancel",
            };

            var service = new SessionService();
            var session = service.Create(options);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users.Find(userId);

            var order = new Models.Order
            {
                StripeSessionId = session.Id,
                TotalAmount = _cartService.GetTotal(),
                OrderStatus = Models.Order.StatusOrder.Recieved,
                FullName = shippingInfo.FullName,
                Address = shippingInfo.Address,
                City = shippingInfo.City,
                Province = shippingInfo.Province,
                PostalCode = shippingInfo.PostalCode,
                UserId = userId,
                DriverUserId = "fd20cb69-01a6-49a5-a16f-e0e35ab37f4e",
                OrderItems = cartItems.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            return Redirect(session.Url);
        }

        public IActionResult Success()
        {
            return View();
        }

        public IActionResult Cancel()
        {
            return View();
        }

        public class ShippingViewModel
        {
            [Required, Display(Name = "Full Name")]
            public string FullName { get; set; }

            [Required, Display(Name = "Address")]
            public string Address { get; set; }

            [Required, Display(Name = "City")]
            public string City { get; set; }

            [Required, Display(Name = "Province")]
            public string Province { get; set; }

            [Required, Display(Name = "Postal Code")]
            public string PostalCode { get; set; }
        }
    }
}
