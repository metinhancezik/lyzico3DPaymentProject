﻿namespace ECommerceView.Models.Cart
{
    public class SyncCartRequestModel
    {
        public List<CartCookieItemModel> CartItems { get; set; }
        public string Token { get; set; }

        public SyncCartRequestModel()
        {
            CartItems = new List<CartCookieItemModel>();
        }
    }
}
