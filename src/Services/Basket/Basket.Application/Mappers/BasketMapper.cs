using Basket.Application.Command;
using Basket.Application.Responses;
using Basket.Core.Entities;

namespace Basket.Application.Mappers;

public static class BasketMapper
{
    public static ShoppingCartResponse ToResponse(this ShoppingCart shoppingCart)
        => MapCart(shoppingCart);

    public static readonly Func<ShoppingCart, ShoppingCartResponse> MapCart = cart => new ShoppingCartResponse
    {
        UserName = cart.UserName,
        Items = cart.Items.Select(x => new ShoppingCartItemResponse
        {
            Quantity = x.Quantity,
            ImageFile = x.ImageFile,
            Price = x.Price,
            ProductId = x.ProductId,
            ProductName = x.ProductName,
        }),
    };

    public static ShoppingCart ToEntity(this CreateShoppingCartCommand createShoppingCartCommand)
        => new ShoppingCart
        {
            UserName = createShoppingCartCommand.UserName,
            Items = createShoppingCartCommand.Items.Select(x => new ShoppingCartItem
            {
                Quantity = x.Quantity,
                ImageFile = x.ImageFile,
                Price = x.Price,
                ProductId = x.ProductId,
                ProductName = x.ProductName,
            }).ToList()
        };
}