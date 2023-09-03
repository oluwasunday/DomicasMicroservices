﻿using Shopping.Aggregator.Extensions;
using Shopping.Aggregator.Models;

namespace Shopping.Aggregator.Services
{
    public class BasketService : IBasketService
    {
        private readonly HttpClient _client; // used to consume APIs

        public BasketService(HttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<BasketModel> GetBasket(string username)
        {
            var response = await _client.GetAsync($"/api/v1/Basket/{username}");
            return await response.ReadContentAs<BasketModel>();
        }
    }
}
