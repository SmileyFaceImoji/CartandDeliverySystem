using CartandDeliverySystem.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;

namespace CartandDeliverySystem.Services
{
    public class CartService
    {
        private const string CartSessionKey = "ShoppingCart";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private List<CartItem> GetCartItems()
        {
            return _httpContextAccessor.HttpContext.Session.GetObject<List<CartItem>>(CartSessionKey)
                ?? new List<CartItem>();
        }

        private void SaveCartItems(List<CartItem> items)
        {
            _httpContextAccessor.HttpContext.Session.SetObject(CartSessionKey, items);
        }

        public void AddItem(Product product, int quantity)
        {
            var items = GetCartItems();
            var existingItem = items.FirstOrDefault(i => i.ProductId == product.Id);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                items.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl,
                    Quantity = quantity
                });
            }

            SaveCartItems(items);
        }

        public bool RemoveItem(int productId)
        {
            var items = GetCartItems();
            var item = items.FirstOrDefault(i => i.ProductId == productId);

            if (item == null) return false;

            items.Remove(item);
            SaveCartItems(items);
            return true;
        }

        public bool UpdateQuantity(int productId, int quantity)
        {
            // Handle zero or negative quantity by removing item
            //We assuming they are removing an item
            if (quantity <= 0)
            {
                return RemoveItem(productId);
            }

            var items = GetCartItems();
            var item = items.FirstOrDefault(i => i.ProductId == productId);

            if (item == null) return false;

            item.Quantity = quantity;
            SaveCartItems(items);
            return true;
        }


        public decimal GetTotal() =>
            GetCartItems().Sum(i => i.Price * i.Quantity);

        public IEnumerable<CartItem> GetItems() => GetCartItems();

        public int ItemCount() => GetCartItems().Sum(i => i.Quantity);

        public void Clear()
        {
            _httpContextAccessor.HttpContext.Session.Remove(CartSessionKey);
        }
    }

    public class CartItem
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public int Quantity { get; set; }

        public decimal Subtotal => Price * Quantity;
    }
}