using CartandDeliverySystem.Data;
using CartandDeliverySystem.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CartandDeliverySystem.Models;

namespace CartandDeliverySystem.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;
        
        public CheckoutController(ApplicationDbContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }
        //Hide this API key in the appsettings.json in production.
        //This is just for simplicity
        private readonly string StripeSecret = "sk_test_51P3ELjFokx3M8JKGyNFMiHZqMrb1xTzAdHuofa7ZXT9YIQQmpihiNnG1lkBjH2CGp7YmEImpnGzUfKSHEWAhV1Vg00bDYVTbxX";

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

            // Save shipping info to session
            HttpContext.Session.SetObject("ShippingInfo", model);

            return RedirectToAction("Pay");
        }

        public IActionResult Pay()
        {
            StripeConfiguration.ApiKey = StripeSecret;
            //getting the cart in its session
            var cartItems = _cartService.GetItems().ToList();
            if (!cartItems.Any()) return RedirectToAction("Index", "Cart");
            //getting shipping details in its session
            // Get shipping info from session
            var shippingInfo = HttpContext.Session.GetObject<ShippingViewModel>("ShippingInfo");
            if (shippingInfo == null) return RedirectToAction("ConfirmAddress");

            var domain = "https://localhost:7155"; // Hardcode your URL
            var options = new SessionCreateOptions
            {
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
            

            // Create order WITH items
            var order = new Models.Order
            {
                StripeSessionId = session.Id,
                TotalAmount = _cartService.GetTotal(),
                OrderStatus = Models.Order.StatusOrder.Recieved,
                //shipping details
                // Shipping details
                FullName = shippingInfo.FullName,
                Address = shippingInfo.Address,
                City = shippingInfo.City,
                Province = shippingInfo.Province,
                PostalCode = shippingInfo.PostalCode,
                UserId = userId,
                //order items
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

        // STEP 3: Handle results
        public IActionResult Success()
        {
            return View(); // Just show a success page
        }

        public IActionResult Cancel()
        {
            return View(); // Just show a cancel page
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
