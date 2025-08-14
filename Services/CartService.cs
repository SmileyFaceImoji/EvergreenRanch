using EvergreenRanch.Models;
using EvergreenRanch.Utilities;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;

namespace EvergreenRanch.Services
{
    public class CartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string SessionKeyCart = "CartItems";

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void AddToCart(int animalId)
        {
            var cart = GetCartItems();
            if (!cart.Contains(animalId))
            {
                cart.Add(animalId);
                SaveCart(cart);
            }
        }

        public void RemoveFromCart(int animalId)
        {
            var cart = GetCartItems();
            if (cart.Contains(animalId))
            {
                cart.Remove(animalId);
                SaveCart(cart);
            }
        }

        public List<int> GetCartItems()
        {
            return _httpContextAccessor.HttpContext.Session.GetObject<List<int>>(SessionKeyCart)
                ?? new List<int>();
        }

        public void ClearCart()
        {
            _httpContextAccessor.HttpContext.Session.Remove(SessionKeyCart);
        }

        public int GetCartItemCount()
        {
            return GetCartItems().Count;
        }

        private void SaveCart(List<int> cart)
        {
            _httpContextAccessor.HttpContext.Session.SetObject(SessionKeyCart, cart);
        }
    }
}