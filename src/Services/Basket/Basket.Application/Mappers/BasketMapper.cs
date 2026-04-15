using Basket.Application.Commands;
using Basket.Application.DTOs;
using Basket.Application.Responses;
using Basket.Core.Entities;
using EventBus.Messages.Events;

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

    public static ShoppingCart ToEntity(this ShoppingCartResponse shoppingCart)
        => new ShoppingCart(shoppingCart.UserName)
        {
            Items = shoppingCart.Items.Select(x => new ShoppingCartItem
            {
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                Price = x.Price,
                Quantity = x.Quantity,
            }).ToList()
        };

    public static BasketCheckoutEvent ToBasketCheckoutEvent(this BasketCheckoutDto basketCheckoutDto,
        ShoppingCart basket)
        => new BasketCheckoutEvent
        {
            UserName = basketCheckoutDto.UserName,
            TotalPrice = basketCheckoutDto.TotalPrice,
            Name = basketCheckoutDto.Name,
            EmailAddress = basketCheckoutDto.EmailAddress,
            AddressLine = basketCheckoutDto.AddressLine,
            Country = basketCheckoutDto.Country,
            State = basketCheckoutDto.State,
            ZipCode = basketCheckoutDto.ZipCode,
            CardName = basketCheckoutDto.CardName,
            CardNumber = basketCheckoutDto.CardNumber,
            CardExpiration = basketCheckoutDto.CardExpiration,
            Cvv = basketCheckoutDto.Cvv,
            PaymentMethod = basketCheckoutDto.PaymentMethod,
        };
}