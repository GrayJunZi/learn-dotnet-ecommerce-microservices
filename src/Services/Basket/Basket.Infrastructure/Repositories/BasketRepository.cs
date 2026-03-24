using System.Text.Json;
using Basket.Core.Entities;
using Basket.Core.Repositories;
using Microsoft.Extensions.Caching.Distributed;

namespace Basket.Infrastructure.Repositories;

public class BasketRepository(IDistributedCache cache) : IBasketRepository
{
    public async Task<ShoppingCart> GetBasket(string userName)
    {
        var basket = await cache.GetStringAsync(userName);
        if (string.IsNullOrEmpty(basket))
        {
            return null;
        }
        
        return JsonSerializer.Deserialize<ShoppingCart>(basket);
    }

    public async Task<ShoppingCart> UpdateBasket(ShoppingCart shoppingCart)
    {
        await cache.SetStringAsync(shoppingCart.UserName, JsonSerializer.Serialize(shoppingCart));
        return await GetBasket(shoppingCart.UserName);
    }

    public async  Task DeleteBasket(string userName)
    {
        await cache.RemoveAsync(userName);
    }
}